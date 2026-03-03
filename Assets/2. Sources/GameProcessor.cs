using System.Collections;
using Unity.Burst;
using Unity.Cinemachine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[BurstCompile]
public struct TargetSearchJob : IJobParallelFor
{
	[ReadOnly] public NativeArray<float3> EnemyPositions;
	[ReadOnly] public NativeArray<float3> PlayerPositions;
	public NativeArray<int> NearestTargetIndices;

	public void Execute(int index)
	{
		var enemyPosition = EnemyPositions[index];
		var minDistanceSqr = float.MaxValue;
		int nearestIndex = -1;

		for (int i = 0; i < PlayerPositions.Length; i++)
		{
			var distanceSq = math.distancesq(enemyPosition, PlayerPositions[i]);
			if (distanceSq < minDistanceSqr)
			{
				minDistanceSqr = distanceSq;
				nearestIndex = i;
			}
		}

		NearestTargetIndices[index] = nearestIndex;
	}
}


/*
 * 게임 진행은 host가 전담한다
 */
public class GameProcessor : NetworkBehaviour
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
	CinemachineCamera _cineCamera;


	UILevelSO _uiso;
	PlayerInfoSO _playerInfo;
	Coroutine _updateCo;
	Coroutine _enemyTargetSettingCo;

	string[] _weaponNames = new[] {
		"shield",
		"shock",
		"homing",
		"cluster",
		"wave",
		"laser",
		"bolt"
	};

	public event UnityAction OnSceneLoadRequested;


	void Awake()
	{
		if (FindAnyObjectByType<UILevelSOHolder>() == null)
		{
			var obj = new GameObject("[UI Level Holder]");
			obj.AddComponent<UILevelSOHolder>();
		}
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_uiso.OnTestEvent += TestEventListner;

		if (IsHost)
		{
			_updateCo = StartCoroutine(GameUpdate());
			_enemyTargetSettingCo = StartCoroutine(EnemyTargetSetting());
		}
	
	}

	IEnumerator EnemyTargetSetting()
	{
		var delay = new WaitForSeconds(1f);
		var smallDelay = new WaitForSeconds(0.02f);
		NativeArray<float3> playerPositions;
		NativeArray<float3> enemyPositions;
		NativeArray<int> nearestIndices; // enemy 가 target으로 지정하는

		while (true)
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
				NearestTargetIndices = nearestIndices
			};

			var handle = job.Schedule(enemyCount, 64);
			//while (!handle.IsCompleted)
			//{
			//	yield return smallDelay;
			//}
			handle.Complete();
			//string log = "indices ";
			//for (int i = 0; i < enemyCount; i++)
			//{
			//	log += $" {nearestIndices[i]}";
			//}
			//GLogger.Log(log);

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

	/*
	 * 게임 진행을 위한 코드를 작성한다
	 * 50ms 마다 게임 진행을 업데이트한다.
	 */
	IEnumerator GameUpdate()
	{
		yield return null;
		var delay = new WaitForSeconds(0.05f);

		while (true)
		{
			

			yield return delay;
		}
	}

	void TestEventListner(int index)
	{
		if (index == 0)
		{
			_playerSpawner.SpawnPlayerRpc(
				Vector2.zero,
				Quaternion.identity,
				new PlayerInstantiateData()
				{
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
			var random = UnityEngine.Random.Range(-0.5f, 0.5f);
			var random2 = UnityEngine.Random.Range(-0.5f, 0.5f);
			_itemSpawner.SpawnItemRpc(
				new Vector2(random, random2),
				Quaternion.identity,
				new ItemInstantiateData()
				{
					ItemType = (int)ItemType.Weapon,
					ItemEffect = _weaponNames[(int)UnityEngine.Random.Range(0f, 6f)]
				});
		}
		else if (index == 3)
		{
			_itemSpawner.DespawnAllItemsRpc();
		}
		else if (index == 4)
		{
			_enemySpawner.SpawnEnemyRpc(
				new Vector2(4f, UnityEngine.Random.Range(-3f, 3f)),
				Quaternion.identity,
				new EnemyInstantiateData()
				{
					PrefabId = "SuicideBomber",
				});
		}
	}
}
