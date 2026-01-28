using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Multiplayer.Playmode;
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
	[SerializeField]
	ChatBoxSynchronizer _chatBoxSync;
	[SerializeField]
	string _nextSceneName;

#if UNITY_EDITOR
	[SerializeField]
	MPPMNGOTestSO _ngoTestSO;
#endif

	Coroutine _taskCo;
	Coroutine _notifyCo;
	Lobby _lobby;
	ILobbyEvents _lobbyEvents;
	uint _slotIndex;
	bool _ready;
	bool _connected;

	void Awake()
	{
		if (FindAnyObjectByType<UISessionSOHolder>() == null)
		{
			var obj = new GameObject("[UI Session Holder]");
			obj.AddComponent<UISessionSOHolder>();
		}

#if UNITY_EDITOR
		if (_ngoTestSO.IsOn)
		{
			GLogger.LogWarning("=== START NGO TEST ===");

			var obj = new GameObject("[Player Info Holder]");
			var piHolder = obj.AddComponent<PlayerInfoSOHolder>();
			DontDestroyOnLoad(obj);

			return;
		}
#endif
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.Notification = FindAnyObjectByType<UINotification>();
		_playerSlotSync = FindAnyObjectByType<PlayerSlotSynchronizer>();
		_playerSlotSync.OnSlotDataChanged += OnSlotDataChanged;
		_chatBoxSync.OnReceivedChatMessage += OnReceivedChatMessage;
		_uiso.OnClickReady += OnReady;
		_uiso.OnClickLeave += OnLeave;
		_uiso.OnSubmitMessage += OnSubmitMessage;

		_networkEventHandler.OnClientConnected += OnClientConnected;
		_networkEventHandler.OnClientDisconnected += OnClientDisconnected;
		_networkEventHandler.OnPeerConnected += OnPeerConnected;
		_networkEventHandler.OnPeerDisconnected += OnPeerDisconnected;
		_networkEventHandler.OnSceneEvent += OnSceneEvent;

#if UNITY_EDITOR
		if (_ngoTestSO.IsOn)
		{
			StartCoroutine(StartNGOTestClient());
			return;
		}
#endif

		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_sessionParameter = FindAnyObjectByType<SessionParameterSOHolder>().Data;
		if (_sessionParameter == null)
		{
			LoadScene("LobbyScene");
		}

		if (_sessionParameter.LobbyName != null)
		{
			// start as host
			StartCoroutine(StartHostCo());
			_uiso.SessionCommunication.SetReadyButtonText("START GAME");
			_uiso.SessionCommunication.SetReadyButtonHighlight(true);
		}
		else
		{
			// start as peer
			StartCoroutine(StartClientCo());
			_uiso.SessionCommunication.SetReadyButtonText("READY");
			_uiso.SessionCommunication.SetReadyButtonHighlight(false);
		}

		StartCoroutine(ConnectionWatchDog());
	}

#if UNITY_EDITOR
	IEnumerator StartNGOTestClient()
	{
		yield return null;
		if (CurrentPlayer.IsMainEditor)
		{
			NetworkManager.Singleton.StartHost();
		}
		else
		{
			NetworkManager.Singleton.StartClient();

		}
	}
#endif

	IEnumerator ConnectionWatchDog()
	{
		yield return null;
		var delay = new WaitForSeconds(1f);
		int count = 10;
		while (count > 0)
		{
			yield return delay;

			if (_connected)
			{
				yield break;
			}

			count--;
		}

		if (_lobby != null)
		{
			var task = UGSLobbyManager.RemovePlayer(_lobby.Id);
			yield return new WaitUntil(() => task.IsCompleted);
		}

		ShowNotification("session-no-response-from-host");
		yield return new WaitForSeconds(3f);
		ShowNotification("session-no-response-from-host-guide");
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
		
		_uiso.SetInteractable(true);
		StartCoroutine(HeartbeatLobby(lobby.Id));

		GLogger.Log($"StartHost Lobby creation is successful! host id:{lobby.HostId} lobby id: {lobby.Id}");
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
	}

	void OnClientConnected(ulong clientId)
	{
		GLogger.Log($"OnClientConnected {clientId}");

		if (clientId == NetworkManager.Singleton.LocalClientId)
		{
#if UNITY_EDITOR
			_playerInfo.Nickname = $"{clientId}_client";

			switch (clientId)
			{
				case 0: _playerInfo.PersonalColor = Color.red; break;
				case 1: _playerInfo.PersonalColor = Color.yellow; break;
				case 2: _playerInfo.PersonalColor = Color.green; break;
				case 3: _playerInfo.PersonalColor = Color.cyan; break;
			}
#endif
			_networkEventHandler.SetSceneEventListner();
			_connected = true;
			_playerSlotSync.AddClientRpc(clientId, _playerInfo.Nickname, _playerInfo.PersonalColor);
		}
	}

	void OnClientDisconnected(ulong clientId)
	{
		GLogger.Log($"OnClientDisconnected {clientId}");
#if UNITY_EDITOR
		if (_ngoTestSO.IsOn)
		{
			return;
		}
#endif
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

	void OnSceneEvent(SceneEventType eventType, ulong clientId)
	{
		GLogger.Log($"SceneEvent {eventType} {clientId}");
		switch (eventType)
		{
			case SceneEventType.Unload:
				if (clientId == NetworkManager.Singleton.LocalClientId)
				{
					CleanEvent();
				}

				break;
		}
	}

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
			bool thisSlotIsMe = data.Nickname == _playerInfo.Nickname;
			_uiso.PlayerSlotManager.SetReadyState(i, data.Ready, thisSlotIsMe);
			if (i > 0 && thisSlotIsMe)
			{
				_uiso.SessionCommunication.SetReadyButtonHighlight(data.Ready);
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

	void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
	{
		switch (state)
		{
			case LobbyEventConnectionState.Unsubscribed:
				/* Update the UI if necessary, as the subscription has been stopped. */
				break;
			case LobbyEventConnectionState.Subscribing:
				/* Update the UI if necessary, while waiting to be subscribed. */
				break;
			case LobbyEventConnectionState.Subscribed:
				/* Update the UI if necessary, to show subscription is working. */
				break;
			case LobbyEventConnectionState.Unsynced:
				/* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */
				break;
			case LobbyEventConnectionState.Error:
				/* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */
				break;
		}
	}

	void OnSubmitMessage(string message)
	{
		//GLogger.Log($"message: {message}");
		//_uiso.AddMessage("aabbcc", message, 
		//	new Color((int)Random.Range(128, 255), 
		//	(int)Random.Range(128, 255), 
		//	(int)Random.Range(128, 255)));
		_chatBoxSync.ChatMessageRpc(_playerInfo.Nickname, message, _playerInfo.PersonalColor);
	}

	void OnReady()
	{
		GLogger.Log("OnReady");
		if (!_connected)
		{
			GLogger.LogWarning("OnReady Not connected");
			return;
		}

		if (NetworkManager.Singleton.IsHost)
		{
			var count = _playerSlotSync.GetSlotDataCount();
			var ready = 0;

			for (int i = 0; i < count; i++)
			{
				if (_playerSlotSync.GetSlotData(i).Ready)
				{
					ready++;
				}
			}
			GLogger.Log($"ready = {ready} / count = {count}");
			if (count == ready + 1)
			{
				GLogger.LogWarning("ALL READY. START GAME");

				LoadSceneNetwork(_nextSceneName);
			}
		}
		else
		{
			_ready = !_ready;
			_playerSlotSync.ReadyClientRpc(NetworkManager.Singleton.LocalClientId, _ready);
		}
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
			if (NetworkManager.Singleton.IsHost)
			{
				GLogger.LogWarning($"Delete Lobby {_lobby.Id}");
				var task = UGSLobbyManager.DeleteLobby(_lobby.Id);
				yield return new WaitUntil(() => task.IsCompleted);
			}
			else
			{
				var task = UGSLobbyManager.RemovePlayer(_lobby.Id);
				yield return new WaitUntil(() => task.IsCompleted);
			}
		}

		NetworkManager.Singleton.Shutdown();
		LoadScene("LobbyScene");
	}

	void LoadScene(string sceneName)
	{
		CleanEvent();
		StopAllCoroutines();
		SceneManager.LoadScene(sceneName);
	}

	void LoadSceneNetwork(string sceneName)
	{
		CleanEvent();
		StopAllCoroutines();
		var status = NetworkManager.Singleton.SceneManager.LoadScene(
					_nextSceneName, LoadSceneMode.Single);

		if (status != SceneEventProgressStatus.Started)
		{
			GLogger.LogWarning($"Failed to load scene TestGameScene. {status}");
		}
	}

	void CleanEvent()
	{
		_uiso.ClearEvent();
		_networkEventHandler.ClearConnectionEventListner();

#if UNITY_EDITOR
		if (_ngoTestSO.IsOn)
		{
			return;
		}
#endif

		_tcpClient.OnReceived -= OnTCPDataReceived;
	}

	void OnReceivedChatMessage(string speaker,string message, Color color)
	{
		//GLogger.Log($"OnReceivedChatMessage {speaker} {message} {color}");
		_uiso.AddMessage(speaker, message, color);
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
