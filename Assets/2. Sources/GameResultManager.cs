using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameResultManager : NetworkBehaviour
{
	UIGameResultSO _uiso;
	GameResultSO _gameResult;

	void Awake()
	{
		if (FindAnyObjectByType<UIGameResultSOHolder>() == null)
		{
			var obj = new GameObject("[UI Level Holder]");
			obj.AddComponent<UIGameResultSOHolder>();
		}
	}

	void Start()
	{
		var neh = FindAnyObjectByType<NetworkEventHandler>();
		if (neh != null)
		{
			Destroy(neh);
		}
	}

	public override void OnNetworkSpawn()
	{
		_uiso = FindAnyObjectByType<UIGameResultSOHolder>().Data;
		_uiso.OnExitToLobby += OnExitToLobby;
		_uiso.Curtain = FindAnyObjectByType<UICurtain>();
		
		if (IsHost)
		{
			_gameResult = FindAnyObjectByType<GameResultSOHolder>().Data;
			GLogger.Log($"Game Result {_gameResult.EnemyKilledCount} {_gameResult.PlayerDeadCount}");
		}


		if (NetworkManager.IsHost)
		{
			NetworkManager.SceneManager.OnLoadEventCompleted += OnSceneLoadEventCompleted;
		}
	}

	public override void OnNetworkDespawn()
	{
		GLogger.Log("OnNetworkDespawn");
	}

	public override void OnDestroy()
	{
		base.OnDestroy();
		_uiso.ClearEvent();
	}

	void OnSceneLoadEventCompleted(
		string sceneName,
		LoadSceneMode mode,
		List<ulong> clientsCompleted,
		List<ulong> clientsTimeOut)
	{
		StartCoroutine(WaitForLobbyButton());
	}

	void OnClientDisconnectCallback(ulong id)
	{
		GLogger.LogWarning($"OnClientDisconnectCallback {id}");
	}

	IEnumerator WaitForLobbyButton()
	{
		var delay1 = new WaitForSeconds(1f);
		yield return null;

		OpenCurtainRpc();
		SetGameResultRpc(_gameResult.EnemyKilledCount);
		
		yield return delay1;

		ReadyToLeaveRpc();
		_uiso.ShowExitToLobbyButton();
		NetworkManager.Shutdown();
		
	}

	[Rpc(SendTo.Everyone)]
	void OpenCurtainRpc()
	{
		_uiso.OpenCurtain();
	}

	[Rpc(SendTo.Everyone)]
	void SetGameResultRpc(int result)
	{
		_uiso.SetGameResult(result);
	}

	[Rpc(SendTo.NotMe)]
	void ReadyToLeaveRpc()
	{
		_uiso.ShowExitToLobbyButton();
		NetworkManager.Shutdown();
	}

	void OnExitToLobby()
	{
		GLogger.Log("OnExitToLobby");
		SceneManager.LoadScene("LobbyScene");
	}
}
