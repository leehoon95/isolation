using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;



[Serializable]
public struct CameraConfig
{
	public string CameraName;
	public CinemachineCamera Camera;
}

/*
 * 게임 진행은 host가 전담한다
 */
public class GameProcessor : NetworkBehaviour, IGameProjcessorInterface
{
	[SerializeField]
	LevelGameManager _GM;
	[SerializeField]
	PlayerSpawner _playerSpawner;
	[SerializeField]
	EnemySpawner _enemySpawner;
	[SerializeField]
	ItemSpawner _itemSpawner;
	[SerializeField]
	PooledDynamicSpawner _pds;
	[SerializeField]
	List<CameraConfig> _cameraConfigs;
	[SerializeField]
	bool _automaticClient;
	[Header("Quest")]
	[SerializeField]
	int _questEnemyCountToKill;
	[SerializeField]
	int _spawnEnemyPeriod;

	NetworkEventHandler _networkEventHandler;
	UILevelSO _uiso;
	PlayerInfoSO _playerInfo;
	GameResultSO _gameResult;
	StaticObjectHandler _soh;
	Dictionary<string, CinemachineCamera> _cameras = new();
	bool _isMyPlayerCharacterAlive;
	bool _playing;
	int _enemyKilledCount;
	int _levelProgress;
	bool _firstQuestCompleted;
	bool _notFirstSpawn;
	NetworkVariable<bool> _gameEnd = new NetworkVariable<bool>(
		false,
		readPerm: NetworkVariableReadPermission.Everyone,
		writePerm: NetworkVariableWritePermission.Server);

	string[] _weaponNames = new[] {
		"bolt",
		"missile",
		"laser",
		"burst",
		"shield",
		"heal",
		"bomb",
	};

	EnemyInstantiateData _seid = new EnemyInstantiateData()
	{
		PrefabId = "SuicideBomber",
		Speed = 1.8f,
		MaxHealthPoint = 120
	};

	EnemyInstantiateData _reid = new EnemyInstantiateData()
	{
		PrefabId = "RangedAttacker",
		Speed = 1.5f,
		MaxHealthPoint = 80
	};

	public bool IsMyPlayerCharacterAlive => _isMyPlayerCharacterAlive;

	void Awake()
	{
		if (FindAnyObjectByType<UILevelSOHolder>() == null)
		{
			var obj = new GameObject("[UI Level Holder]");
			obj.AddComponent<UILevelSOHolder>();
		}

		if (FindAnyObjectByType<GameResultSOHolder>() == null)
		{
			var obj = new GameObject("[Game Result]");
			_gameResult = obj.AddComponent<GameResultSOHolder>().Data;
			DontDestroyOnLoad(obj);
		}
	}

	void Start()
	{
		_networkEventHandler = FindAnyObjectByType<NetworkEventHandler>();

		if (_networkEventHandler == null)
		{
			GLogger.LogWarning("NetworkEventHandler is null");
		}
		else
		{
			_networkEventHandler.OnClientConnected += (id) => GLogger.LogWarning($"OnClientConnected {id}"); 
			_networkEventHandler.OnClientDisconnected += (id) => GLogger.LogWarning($"OnClientDisconnected {id}"); 
			_networkEventHandler.OnPeerConnected += (id) => GLogger.LogWarning($"OnPeerConnected {id}"); 
			_networkEventHandler.OnPeerDisconnected += (id) => GLogger.LogWarning($"OnPeerDisconnected {id}");
			_networkEventHandler.OnSceneEvent += (sceneEventType, id) => GLogger.LogWarning($"OnSceneEvent {id} {sceneEventType}");
		}
	}

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		//GLogger.Log("gp OnNetworkPreSpawn");
		Init();
	}

	public override void OnNetworkSpawn()
	{
		//GLogger.Log("gp OnNetworkSpawn");
	}

	void Init()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_gameResult = FindAnyObjectByType<GameResultSOHolder>().Data;

		_gameResult.PlayerDeadCount = 0;
		_uiso.Notification = FindAnyObjectByType<UINotification>();
		_uiso.OnTestEvent += TestEventListner;
		_uiso.Curtain = FindAnyObjectByType<UICurtain>();
		_soh = FindAnyObjectByType<StaticObjectManager_Level_0>();

		_playerSpawner.PlayerSpawned += OnPlayerSpawned;
		_playerSpawner.PlayerDespawned += OnPlayerDespawned;


		foreach (var item in _cameraConfigs)
		{
			_cameras[item.CameraName] = item.Camera;
		}

		if (NetworkManager.IsHost)
		{
			_playing = true;
			NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadEventCompleted;
			_soh.LevelSwitchTriggered += OnLevelSwitchTriggered;
			_enemySpawner.EnemyDespawned += OnEnemyDespawned;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		StopAllCoroutines();
		_uiso.OnTestEvent -= TestEventListner;
		_playerSpawner.PlayerSpawned -= OnPlayerSpawned;
		_playerSpawner.PlayerDespawned -= OnPlayerDespawned;
	}

	public override void OnNetworkDespawn()
	{
		if (NetworkManager.IsHost)
		{
			NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadEventCompleted;
			_soh.LevelSwitchTriggered -= OnLevelSwitchTriggered;
			_enemySpawner.EnemyDespawned -= OnEnemyDespawned;
		}
	}

	IEnumerator EnemyTargetSetting()
	{
		var delay = new WaitForSeconds(1f);
		NativeArray<float3> playerPositions;
		NativeArray<float3> enemyPositions;
		NativeArray<int> nearestIndices; // enemy 가 target으로 지정하는

		while (!_playerSpawner.IsSpawned || !_enemySpawner.IsSpawned)
		{
			yield return delay;
		}

		while (_playing)
		{
			var players = _playerSpawner.GetPlayers();
			var enemys = _enemySpawner.GetEnemys();
			var playerCount = players.Count;
			var enemyCount = enemys.Count;

			if (playerCount == 0 || enemyCount == 0)
			{ 
				yield return delay;
				continue;
			}
			
			playerPositions = new NativeArray<float3>(players.Count, Allocator.TempJob);
			enemyPositions = new NativeArray<float3>(enemys.Count, Allocator.TempJob);
			nearestIndices = new NativeArray<int>(enemys.Count, Allocator.TempJob);
			//GLogger.Log($"=== {playerPositions.Length} {enemyPositions.Length} {nearestIndices.Length}");

			for (int i = 0; i < playerCount; i++)
			{
				playerPositions[i] = players[i].GO.transform.position;
			}

			for (int i = 0; i < enemyCount; i++)
			{
				enemyPositions[i] = enemys[i].GO.transform.position;
			}

			var job = new TargetSearchJob()
			{
				EnemyPositions = enemyPositions,
				PlayerPositions = playerPositions,
				NearestPlayerIndices = nearestIndices
			};

			var handle = job.Schedule(enemyCount, 64);
			handle.Complete();

			for (int i = 0; i < enemyCount;i++)
			{
				enemys[i].Target = players[nearestIndices[i]].GO.transform;
			}

			enemyPositions.Dispose();
			playerPositions.Dispose();
			nearestIndices.Dispose();

			yield return delay;
		}
	}

	IEnumerator GameStart()
	{
		yield return null;


		_itemSpawner.SpawnFieldItems();
		OpenCurtainRpc();

		var startDelay = new WaitForSeconds(1f);

		yield return startDelay;

		SpawnEachPlayerRpc();


		yield return new WaitForSeconds(1);

		ChangeCameraTargetToAimHelperRpc();

		//yield return new WaitForSeconds(1000);

		var enemyTargetingCo = StartCoroutine(EnemyTargetSetting());

		ShowNotification("quest-kill-enemy", _questEnemyCountToKill.ToString());

		var firstRoomSpawnSpots = new Vector2[5]
		{
			_soh.GetSpawnSpot("FR", 0),
			_soh.GetSpawnSpot("FR", 1),
			_soh.GetSpawnSpot("FR", 2),
			_soh.GetSpawnSpot("FR", 3),
			_soh.GetSpawnSpot("FR", 4)
		};

		var t = 20f;
		var delay = new WaitForSeconds(0.05f);

		while (!_firstQuestCompleted)
		{
			if (t > _spawnEnemyPeriod)
			{
				for (int i = 0; i < 2; ++i)
				{
					SpawnEnemy(firstRoomSpawnSpots[0]);
					SpawnEnemy(firstRoomSpawnSpots[1]);
				}

				yield return null;

				for (int i = 0; i < 2; ++i)
				{
					SpawnEnemy(firstRoomSpawnSpots[2]);
					SpawnEnemy(firstRoomSpawnSpots[3]);
				}

				t = 0f;
			}

			t += 0.05f;
			yield return delay;
		}


		_soh.OpenDoor("FirstDoor");
		ShowNotification("guide-hall-way");

		var hwSpawnSpots = new Vector2[6]
		{
			_soh.GetSpawnSpot("HW", 0),
			_soh.GetSpawnSpot("HW", 1),
			_soh.GetSpawnSpot("HW", 2),
			_soh.GetSpawnSpot("HW", 3),
			_soh.GetSpawnSpot("HW", 4),
			_soh.GetSpawnSpot("HW", 5),
		};

		var spawnBackupEnemyCo = StartCoroutine(SpawnBackupEnemy(firstRoomSpawnSpots[4]));
		var previousLevelProgress = -1;
		var hallWaySpawnDelay = new WaitForSeconds(0.02f);

		while (_levelProgress < 6)
		{
			if (previousLevelProgress != _levelProgress)
			{
				GLogger.Log($"Spawn hw enemy {_levelProgress}");
				var index = Mathf.Min(_levelProgress, hwSpawnSpots.Length);
				for (int i = 0; i < 3; i++)
				{
					SpawnEnemy(hwSpawnSpots[index]);
				}

				previousLevelProgress = _levelProgress;
			}

			yield return hallWaySpawnDelay;
		}

		_playing = false;

		yield return enemyTargetingCo;

		_gameEnd.Value = true;

		//yield return new WaitForSeconds(1f);

		_enemySpawner.DespawnAllEnemysRpc();
		_playerSpawner.DespawnAllPlayersRpc();

		ShowNotification("level-completed");

		yield return new WaitForSeconds(3f);

		CloseCurtainRpc();

		yield return new WaitForSeconds(2f);

		_networkEventHandler?.ClearConnectionEventListner();
		NetworkManager.SceneManager.LoadScene("GameResultScene", LoadSceneMode.Single);
	}

	void SpawnEnemy(Vector2 position)
	{
		_enemySpawner.SpawnEnemyRpc(
			position + UnityEngine.Random.insideUnitCircle,
			quaternion.identity,
			UnityEngine.Random.Range(0, 100) > 20 ? _seid : _reid);
	}

	IEnumerator SpawnBackupEnemy(Vector2 spots)
	{
		yield return null;

		var delay = new WaitForSeconds(10f);

		while (true)
		{
			for (int i = 0; i < 4; ++i)
			{
				SpawnEnemy(spots);
			}
			
			yield return delay;
		}
	}

	void OnPlayerSpawned(IPlayerHandler ph)
	{
		if (_gameEnd.Value)
		{
			return;
		}

		/*
		 * player character 생성시 소유권이 host에서 client로 변경되어 IsOwner는 사용하지 못함
		 */
		if (ph.SpawnClientId == NetworkManager.LocalClientId)
		{
			var cam = _cameras["PlayerCamera"];
			cam.Follow = ph.GO.transform;
			var lens = cam.Lens;
			lens.OrthographicSize = 4f;
			_uiso.ShowIndicator(true);
			_isMyPlayerCharacterAlive = true;

			if (!_notFirstSpawn)
			{
				_notFirstSpawn = true;
			}
			else
			{
				ChangeCameraTargetToAimHelper();
			}
		}
	}

	void OnPlayerDespawned(IPlayerHandler ph)
	{
		if (_gameEnd.Value)
		{
			return;
		}

		if (IsHost)
		{
			_gameResult.PlayerDeadCount++;
		}

		if (ph.NO.IsOwner)
		{
			var cam = _cameras["PlayerCamera"];
			cam.Follow = ph.GO.transform;
			var lens = cam.Lens;
			lens.OrthographicSize = 5f;
			_uiso.ShowIndicator(false);
			_isMyPlayerCharacterAlive = false;
		}
	}

	void OnEnemyDespawned(string prefabId, Vector2 position)
	{
		if (_gameEnd.Value)
		{
			return;
		}

		_enemyKilledCount++;
		_gameResult.EnemyKilledCount++;

		if (!_firstQuestCompleted)
		{
			ShowNotification(
				"quest-kill-progress", 
				_enemyKilledCount.ToString(), 
				_questEnemyCountToKill.ToString());
			if (_enemyKilledCount == _questEnemyCountToKill)
			{
				// door is opened
				_firstQuestCompleted = true;
			}
		}

		if (UnityEngine.Random.Range(0, 100) < 33)
		{
			_itemSpawner.SpawnItemRpc(
				position,
				Quaternion.identity,
				new ItemInstantiateData()
				{
					ItemEffect = _weaponNames[UnityEngine.Random.Range(0, 7)]
				});
		}
	}

	void OnLevelSwitchTriggered(string name, int triggered)
	{
		if (triggered == 0)
		{
			return;
		}

		//GLogger.Log($"level switch {name} {triggered}");
		if (name.Contains("LS_"))
		{
			var s = name.Split('_');
			if (s.Length < 2)
			{
				GLogger.LogWarning($"invalid switch name {name}");
				return;
			}

			var index = int.Parse(s[1]);
			_levelProgress = index; // 0 -> 4, 5개
		}
		else if (name == "LES"
			&& triggered == NetworkManager.Singleton.ConnectedClients.Count)
		{
			_levelProgress = 6;
		}
	}

	[Rpc(SendTo.Everyone)]
	void SpawnEachPlayerRpc()
	{
		var automatic = NetworkManager.LocalClientId != 0 ? _automaticClient : false;

		_playerSpawner.SpawnPlayerRpc(
			NetworkManager.Singleton.LocalClientId,
			Vector2.zero + NetworkManager.LocalClientId * Vector2.right,
			Quaternion.identity,
			new PlayerInstantiateData()
			{
				OwnerClientId = NetworkManager.Singleton.LocalClientId,
				Nickname = _playerInfo.Nickname,
				PersonalColor = _playerInfo.PersonalColor,
				AutomaticMotion = automatic
			});
	}

	[Rpc(SendTo.Everyone)]
	void OpenCurtainRpc()
	{
		_uiso.OpenCurtain();
	}

	[Rpc(SendTo.Everyone)]
	void CloseCurtainRpc()
	{
		GLogger.LogWarning("Close curtain");
		_uiso.CloseCurtain();
	}

	[Rpc(SendTo.Everyone)]
	void ChangeCameraTargetToAimHelperRpc()
	{
		ChangeCameraTargetToAimHelper();
	}

	void ChangeCameraTargetToAimHelper()
	{
		var ph = _playerSpawner.GetPlayer(NetworkManager.LocalClientId);
		if (ph == null)
		{
			return;
		}
		var cam = _cameras["PlayerCamera"];
		cam.Follow = ph.CameraTarget;
	}

	void OnSceneLoadEventCompleted(
		string sceneName,
		LoadSceneMode mode, 
		List<ulong> clientsCompleted,
		List<ulong> clientsTimeOut)
	{
		var log = $"OnSceneLoadEventCompleted {sceneName} {mode}\n";
		foreach (var client in clientsCompleted)
		{
			log += $"{client} completed\n";
		}

		foreach (var client in clientsTimeOut)
		{
			log += $"{client} timeout\n";
		}

		_soh.MaxPlayers = clientsCompleted.Count;
		StartCoroutine(GameStart());
	}

	void ShowNotification(string localizationKey, params string[] argumetns)
	{
		StartCoroutine(ShowNotificationCo(localizationKey, argumetns));
	}

	IEnumerator ShowNotificationCo(string localizationKey, params string[] argumetns)
	{
		var task = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			"DefaultStringTable", 
			localizationKey, 
			LocalizationSettings.SelectedLocale,
			FallbackBehavior.UseProjectSettings,
			argumetns);
		yield return task;

		ShowNotificationRpc(task.Result);
	}

	[Rpc(SendTo.Everyone)]
	void ShowNotificationRpc(FixedString128Bytes text)
	{
		_uiso.Notification.ShowNotification(text.ToString());
	}

	void TestEventListner(int index)
	{
		if (index == 0)
		{
			_playerSpawner.SpawnPlayerRpc(
				NetworkManager.Singleton.LocalClientId,
				Vector2.zero,
				Quaternion.identity,
				new PlayerInstantiateData()
				{
					OwnerClientId = NetworkManager.Singleton.LocalClientId,
					Nickname = _playerInfo.Nickname,
					PersonalColor = _playerInfo.PersonalColor,
				});
		}
		else if (index == 1)
		{
			_playerSpawner.DespawnPlayerRpc();
		}
		else if (index == 2)
		{
			for (int i = 0; i < 3; ++i)
			{
				var random = UnityEngine.Random.Range(-2f, 2f);
				var random2 = UnityEngine.Random.Range(-2f, 2f);
				_itemSpawner.SpawnItemRpc(
					new Vector2(random, random2),
					Quaternion.identity,
					new ItemInstantiateData()
					{
						ItemEffect = _weaponNames[UnityEngine.Random.Range(0, 7)]
					});
			}

		}
		else if (index == 3)
		{
			_itemSpawner.DespawnAllItemsRpc();
		}
		else if (index == 4)
		{
			_enemySpawner.SpawnEnemyRpc(
				new Vector2(3f, UnityEngine.Random.Range(-4f, 4f)),
				Quaternion.identity,
				new EnemyInstantiateData()
				{
					PrefabId = "SuicideBomber",
					Speed = 2f,
					MaxHealthPoint = 120
				});
		}
		else if (index == 5)
		{
			_enemySpawner.SpawnEnemyRpc(
				new Vector2(3f, UnityEngine.Random.Range(-4f, 4f)),
				Quaternion.identity,
				new EnemyInstantiateData()
				{
					PrefabId = "RangedAttacker",
					Speed = 1.7f,
					MaxHealthPoint = 80
				});
		}
		else if (index >= 6 && index <= 8)
		{
			foreach (var item in _cameras)
			{
				item.Value.Priority = 0;
			}

			switch (index)
			{
				case 6: _cameras["PlayerCamera"].Priority = 1; break;
				case 7: _cameras["EnemyCamera"].Priority = 1; break;
				case 8: _cameras["TestCamera"].Priority = 1; break;
			}
		}
		else if (index == 9)
		{

		}
	}
}
