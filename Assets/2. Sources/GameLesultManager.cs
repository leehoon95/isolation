using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLesultManager : NetworkBehaviour
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

		//StartCoroutine(WaitForLobbyButton());
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
		Destroy(_gameResult);

		//if (NetworkManager.IsHost)
		//{
		//	NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoadEventCompleted;
		//}
		//else
		//{
		//	NetworkManager.OnClientDisconnectCallback -= OnClientDisconnectCallback;
		//}
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
		var delay3 = new WaitForSeconds(3f);
		yield return null;

		OpenCurtainRpc();
		SetGameResultRpc(_gameResult.EnemyKilledCount);
		
		yield return delay3;

		ShowExitToLobbyButtonAndShutdownRpc();
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

	[Rpc(SendTo.Everyone)]
	void ShowExitToLobbyButtonAndShutdownRpc()
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
