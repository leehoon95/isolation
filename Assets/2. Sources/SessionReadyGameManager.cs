using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class SessionGameReadyManager : MonoBehaviour
{
	UISessionSO _uiso;
	TCPClientSO _tcpClient;
	PlayerInfoSO _playerInfo;
	SessionParameterSO _sessionParameter;
	PlayerSlotSynchronizer _playerSlotSync;

	[SerializeField]
	NetworkEventHandler _networkEventHandler;

	Coroutine _taskCo;
	Coroutine _notifyCo;
	Lobby _lobby;
	ILobbyEvents _lobbyEvents;
	uint _slotIndex;
	bool _ready;

	void Awake()
	{
		if (FindAnyObjectByType<UISessionSOHolder>() == null)
		{
			var obj = new GameObject("[UI Session Holder]");
			obj.AddComponent<UISessionSOHolder>();
		}

		if (FindAnyObjectByType<SessionParameterSOHolder>() == null)
		{
			GLogger.LogError("SessionGameManger.Awake This session is invalid");
			SceneManager.sceneLoaded += SceneLoadedOnInvalidSession;
		}
	}
	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_sessionParameter = FindAnyObjectByType<SessionParameterSOHolder>().Data;
		_playerSlotSync = FindAnyObjectByType<PlayerSlotSynchronizer>();
		_playerSlotSync.OnSlotDataChanged += OnSlotDataChanged;

		_uiso.OnClickReady += OnReady;
		_uiso.OnClickLeave += OnLeave;
		_uiso.OnSubmitMessage += OnSubmitMessage;
		_networkEventHandler.OnClientConnected += OnClientConnected;
		_networkEventHandler.OnClientDisconnected += OnClientDisconnected;
		_networkEventHandler.OnPeerConnected += OnPeerConnected;
		_networkEventHandler.OnPeerDisconnected += OnPeerDisconnected;

		if (_sessionParameter.LobbyName != null)
		{
			// start as host
			_taskCo = StartCoroutine(StartHostCo());
			_uiso.HideReadyButton();
		}
		else
		{
			// start as peer
			_taskCo = StartCoroutine(StartClientCo());
		}
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneLoadedOnInvalidSession;
	}

	void SceneLoadedOnInvalidSession(Scene scene, LoadSceneMode mode)
	{
		SceneManager.LoadScene("LobbyScene");
	}

	IEnumerator StartHostCo()
	{
		yield return null;
		_uiso.SetInteractable(false);

		var taskRelay = UGSRelayManager.StartHostAndGetJoinCode(
			_sessionParameter.MaxPlayers - 1, "dtls");
		yield return new WaitUntil(() => taskRelay.IsCompleted);

		(var resultRelay, var joincode) = taskRelay.Result;

		if (!resultRelay)
		{
			GLogger.LogError("Failed to start Host");
			OnLeave();
			yield break;
		}

		//var callbacks = new LobbyEventCallbacks();
		//callbacks.LobbyChanged += OnLobbyChanged;
		//callbacks.KickedFromLobby += OnKickedFromLobby;
		//callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
		
		var taskLobby = UGSLobbyManager.CreateLobby(
			_sessionParameter.LobbyName,
			_sessionParameter.MaxPlayers,
			joincode,
			null);
		yield return new WaitUntil(() => taskLobby.IsCompleted);

		(var resultLobby, var lobby, var lobbyEvent) = taskLobby.Result;

		if (resultLobby == false)
		{
			GLogger.LogError("StartHost Failed to create lobby");
			OnLeave();
			yield break;
		}
		
		_lobby = lobby;
		_lobbyEvents = lobbyEvent;

		StartCoroutine(HeartbeatLobby(lobby.Id));

		GLogger.Log($"StartHost Lobby creation is successful! host id:{lobby.HostId} lobby id: {lobby.Id}");

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	IEnumerator StartClientCo()
	{
		yield return null;
		_uiso.SetInteractable(false);

		var task = UGSLobbyManager.GetLobbyById(_sessionParameter.LobbyId);
		yield return new WaitUntil(() => task.IsCompleted);

		var lobby = task.Result;
		if (lobby == null)
		{
			GLogger.LogError($"StartClientCo Faield to get lobby by id({_sessionParameter.LobbyId})");
		}

		_lobby = lobby;

		var isPlaying = lobby.Data["Playing"].Value;
		var relayJoincode = lobby.Data["RelayJoinCode"].Value;

		GLogger.Log($"Lobby Data {isPlaying} {relayJoincode}");

		var task2 = UGSRelayManager.StartClient(relayJoincode, "dtls");
		yield return new WaitUntil(() => task2.IsCompleted);
		
		if (!task2.Result)
		{
			GLogger.LogError("StartClientCo Failed to start client");
			OnLeave();
			yield break;
		}

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	void OnClientConnected(ulong clientId)
	{
		GLogger.Log($"OnClientConnected {clientId}");

		// host에게 player data를 전달
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			_playerSlotSync.AddClientRpc(clientId, _playerInfo.Nickname, _playerInfo.PersonalColor);
		}
	}

	void OnClientDisconnected(ulong clientId)
	{
		GLogger.Log($"OnClientDisconnected {clientId}");
		// 접속한 client만 호출한다
		if (NetworkManager.Singleton.IsHost)
		{
			_playerSlotSync.RemoveClientRpc(clientId);
		}

		// client가 나가기를 눌렀거나 host와 연결이 끊어짐
		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
			OnLeave();
		}
	}

	void OnPeerConnected(ulong clientid)
	{}

	void OnPeerDisconnected(ulong clientid)
	{}

	void OnSlotDataChanged(int count)
	{
		//GLogger.Log($"OnSlotChanged count: {count}");

		for (int i = 0; i < count; i++)
		{
			
			var data = _playerSlotSync.GetSlotData(i);
			_uiso.PlayerSlotManager.SetSlotData(
				i,
				data.Nickname.ToString(),
				data.PersonalColor);
			_uiso.PlayerSlotManager.SetReadyState(i, data.Ready);
			if (data.Nickname == _playerInfo.Nickname)
			{
				_uiso.PlayerSlotManager.SetIsYou(i);
			}
		}

		while (count < 3)
		{
			_uiso.PlayerSlotManager.EmptySlot(count);
			count++;
		}
	}

	IEnumerator LockInteractabilityUntilTaskComplete(Task task)
	{
		_uiso.SetInteractable(false);
		yield return new WaitUntil(() => task.IsCompleted);
		_uiso.SetInteractable(true);
	}

	IEnumerator LockInteractabilityUntilTaskComplete(IEnumerator co)
	{
		_uiso.SetInteractable(false);
		yield return co;
		_uiso.SetInteractable(true);
	}

	IEnumerator HeartbeatLobby(string lobbyId)
	{
		if (lobbyId.IsNullOrEmpty())
		{
			GLogger.LogError("HeartbeatLobby lobbyId is null oro empty");
			yield break;
		}

		var delay = new WaitForSeconds(20f);

		while (true)
		{
			_ = UGSLobbyManager.MaintainLobbyAlive(lobbyId);
			yield return delay;
		}
	}

	void OnLobbyChanged(ILobbyChanges changes)
	{
		return;
		changes.ApplyToLobby(_lobby);

#if UNITY_EDITOR
		string log = "Players\n";
		foreach (var p in _lobby.Players)
		{
			log += $"	{p.Data["Nickname"].Value} {p.Data["PersonalColor"].Value}\n";
		}
		log += "end...";
		GLogger.Log(log);
#endif
	}

	//IEnumerator AddPlayerSlot(string lobbyId)
	//{
	//	//var task = UGSLobbyManager.GetLobbyById(lobbyId);
	//	//yield return new WaitUntil(() => task.IsCompleted);
		
	//	//var lobby = task.Result;
	//	//if (lobby == null)
	//	//{
	//	//	GLogger.LogError($"UpdateLobby failed to get lobby. lobby id: {lobby.Id}");
	//	//	yield break;
	//	//}

	//	var count = _playerSlotSync.GetSlotDataCount();
	//	for (int i = 0; i < count; i++)
	//	{
	//		_playerSlotSync.AddSlotDataRpc(
	//			p.Data["Nickname"].Value, 
	//			PlayerInfoSO.DeserializePersonalColor(p.Data["PersonalColor"].Value));
	//	}

	//	_lobby = lobby;
	//}

	void OnKickedFromLobby()
	{
		GLogger.LogWarning("OnKickedFromLobby");
		StopAllCoroutines();

		StartCoroutine(LockInteractabilityUntilTaskComplete(LeaveFromThisSessionCo()));
	}

	void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
	{
		switch (state)
		{
			case LobbyEventConnectionState.Unsubscribed:
				/* Update the UI if necessary, as the subscription has been stopped. */
				//Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Unsubscribed");
				break;
			case LobbyEventConnectionState.Subscribing:
				/* Update the UI if necessary, while waiting to be subscribed. */
				//Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Subscribing");
				break;
			case LobbyEventConnectionState.Subscribed:
				/* Update the UI if necessary, to show subscription is working. */
				//Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Subscribed");
				break;
			case LobbyEventConnectionState.Unsynced:
				/* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */
				//GLogger.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Unsynced");
				break;
			case LobbyEventConnectionState.Error:
				/* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */
				//GLogger.LogError("NGOGameManager.OnLobbyEventConnectionStateChanged Error");
				break;
		}
	}

	void OnSubmitMessage(string message)
	{
		GLogger.Log($"message: {message}");
		_uiso.AddMessage("aabbcc", message, 
			new Color((int)Random.Range(128, 255), (int)Random.Range(128, 255), (int)Random.Range(128, 255)));
	}

	void OnReady()
	{
		GLogger.Log("Ready");
		_ready = !_ready;
		_playerSlotSync.ReadyClientRpc(NetworkManager.Singleton.LocalClientId, _ready);
	}

	void OnLeave()
	{
		GLogger.Log("OnLeave");
		StartCoroutine(LockInteractabilityUntilTaskComplete(LeaveFromThisSessionCo()));
	}

	IEnumerator LeaveFromThisSessionCo()
	{
		if (_lobbyEvents != null)
		{
			var task = _lobbyEvents.UnsubscribeAsync();
			yield return new WaitUntil(() => task.IsCompleted);
		}

		if (_lobby != null)
		{
			var task = UGSLobbyManager.RemovePlayer(_lobby.Id);
			yield return new WaitUntil(() => task.IsCompleted);
		}

		if (NetworkManager.Singleton.IsHost)
		{
			GLogger.LogWarning($"Delete Lobby {_lobby.Id}");
			UGSLobbyManager.DeleteLobby(_lobby.Id);
		}

		NetworkManager.Singleton.Shutdown();
		LoadScene("LobbyScene");
	}

	void LoadScene(string sceneName)
	{
		_uiso.ClearEvent();
		_tcpClient.OnReceived -= OnTCPDataReceived;
		StopAllCoroutines();
		SceneManager.LoadScene(sceneName);
	}

	async Task OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			await Awaitable.MainThreadAsync();
			LoadScene("LoginScene");

			return;
		}
	}

	void ShowNotification(string localizationKey)
	{
		if (_notifyCo != null)
		{
			StopCoroutine(_notifyCo);
		}

		_notifyCo = StartCoroutine(ShowNotificationCo(localizationKey));
	}

	IEnumerator ShowNotificationCo(string localizationKey)
	{
		var task = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			"DefaultStringTable", localizationKey, LocalizationSettings.SelectedLocale);
		yield return task;

		_uiso.Notification.ShowNotification(task.Result);

		_notifyCo = null;
	}
}
