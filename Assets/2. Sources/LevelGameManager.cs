using Unity.Netcode;
using UnityEngine;

public class StageGameManager : MonoBehaviour
{
	[SerializeField]
	PlayerSpawner _playerSpawner;
	[SerializeField]
	PooledDynamicSpawner _pds;

	PlayerInfoSO _playerInfo;
	UIGameSO _uiso;
	NetworkEventHandler _networkEventHandler;

	void Awake()
	{
		if (FindAnyObjectByType<UIGameSOHolder>() == null)
		{
			var obj = new GameObject("[UI Game Holder]");
			obj.AddComponent<UIGameSOHolder>();
		}
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UIGameSOHolder>().Data;

		_uiso.OnTestEvent += TestEventListner;
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
