using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
	[SerializeField]
	GameProcessor _gameProcessor;

	PlayerInfoSO _playerInfo;
	UILevelSO _uiso;
	NetworkEventHandler _networkEventHandler;

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

		//_networkEventHandler.
	}
}
