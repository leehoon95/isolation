using System.Collections;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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

	[SerializeField]
	NetworkEventHandler _networkEventHandler;

	Coroutine _taskCo;
	Coroutine _notifyCo;
	Lobby _lobby;
	ILobbyEvents _lobbyEvents;

	void Awake()
	{
		if (FindAnyObjectByType<UISessionSOHolder>() == null)
		{
			var obj = new GameObject("[UI Session Holder]");
			obj.AddComponent<UISessionSOHolder>();
		}

		if (FindAnyObjectByType<TCPClientSOHolder>() == null)
		{
			var obj = new GameObject("[TCP Client Holder]");
			obj.AddComponent<TCPClientSOHolder>();
			DontDestroyOnLoad(obj);
		}

		if (FindAnyObjectByType<SessionParameterSOHolder>() == null)
		{
			GLogger.LogError("SessionGameManger.Awake This session is invalid");
			SceneManager.sceneLoaded += SceneLoadedOnInvalidSession;
		}
	}

	void Start()
	{
		//_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_sessionParameter = FindAnyObjectByType<SessionParameterSOHolder>().Data;

		_uiso.OnSubmitMessage += OnSubmitMessage;
		_uiso.OnClickLeave += LeaveFromSession;
		_networkEventHandler.OnClientConnected += OnClientConnected;
		_networkEventHandler.OnClientDisconnected += OnClientDisconnected;
		_networkEventHandler.OnPeerConnected += OnPeerConnected;
		_networkEventHandler.OnPeerDisconnected += OnPeerDisconnected;

		if (_sessionParameter.LobbyName != null)
		{
			// start as host
			_taskCo = StartCoroutine(StartHostCo());
			
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
			LeaveFromSession();
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
			LeaveFromSession();
			yield break;
		}
		
		_lobby = lobby;
		_lobbyEvents = lobbyEvent;

		_uiso.PlayerSlotManager.SetPlayer(
			0,
			_lobby.Players[0].Data["Nickname"].Value,
			PlayerInfoSO.DeserializePersonalColor(_lobby.Players[0].Data["PersonalColor"].Value),
			true);

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
			LeaveFromSession();
			yield break;
		}

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	void OnClientConnected(ulong clientId)
	{
		GLogger.Log($"OnClientConnected {clientId}");
		if (NetworkManager.Singleton.LocalClientId != 0)
		{
			StartCoroutine(UpdatePlayerSlot(_lobby.Id));
		}
	}

	void OnClientDisconnected(ulong clientId)
	{
		GLogger.Log($"OnClientDisconnected {clientId}");

		LeaveFromSession();
	}

	void OnPeerConnected(ulong clientid)
	{
		GLogger.Log($"OnPeerConnected {clientid}");

		StartCoroutine(UpdatePlayerSlot(_lobby.Id));
	}

	void OnPeerDisconnected(ulong clientid)
	{
		GLogger.Log($"OnPeerDisconnected {clientid}");

		StartCoroutine(UpdatePlayerSlot(_lobby.Id));
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

	IEnumerator UpdatePlayerSlot(string lobbyId)
	{
		var task = UGSLobbyManager.GetLobbyById(lobbyId);
		yield return new WaitUntil(() => task.IsCompleted);
		
		Lobby lobby = task.Result;
		if (lobby == null)
		{
			GLogger.LogError($"UpdateLobby failed to get lobby. lobby id: {lobby}");
			yield break;
		}

		uint index = 0;
		foreach (var p in lobby.Players)
		{
			_uiso.PlayerSlotManager.SetPlayer(
				index, 
				p.Data["Nickname"].Value, 
				PlayerInfoSO.DeserializePersonalColor(p.Data["PersonalColor"].Value), 
				index == 0);
			index++;
		}

		while (index < 4)
		{
			_uiso.PlayerSlotManager.RemovePlayer(index);
			index++;
		}

		_lobby = lobby;
	}

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

	void LeaveFromSession()
	{
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
