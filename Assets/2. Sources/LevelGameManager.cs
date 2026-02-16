using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
	[SerializeField]
	PooledDynamicSpawner _pds;
	[SerializeField]
	CinemachineCamera _cineCamera;

	PlayerSpawner _playerSpawner;
	PlayerInfoSO _playerInfo;
	UILevelSO _uiso;
	NetworkEventHandler _networkEventHandler;

	void Awake()
	{
		if (FindAnyObjectByType<UILevelSOHolder>() == null)
		{
			var obj = new GameObject("[UI Game Holder]");
			obj.AddComponent<UILevelSOHolder>();
		}
		
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;

		_uiso.OnTestEvent += TestEventListner;
	}

	public void NotifyPlayerSpawnerSpawned(PlayerSpawner playerSpawner)
	{
		_playerSpawner = playerSpawner;
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
	}
}
