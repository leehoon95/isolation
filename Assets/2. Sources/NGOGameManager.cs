using Google.Protobuf;
using System;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NGOGameManager : MonoBehaviour
{
	[SerializeField]
	GameObject _targetPrefab;
	[SerializeField]
	PlayerSpawner _playerSpawner;
	[SerializeField]
	PooledDynamicSpawner _pds;

	UINGOTestSO _uiso;
	PlayerInfoSO _playerInfo;
	TCPClientSO _tcpClient;
	
	int _pingPongCount;
	bool _isHost;
	string _sessionName;
	string _joinCode;
	string _password;
	Lobby _lobby;
	ILobbyEvents _lobbyEvents;
	Coroutine _heartbeatCoroutin;
	int _maxPeerConnection = 2;

	void Awake()
	{
		if (FindAnyObjectByType<UINGOTestSOHolder>() == null)
		{
			var obj = new GameObject("[UI Lobby Holder]");
			obj.AddComponent<UINGOTestSOHolder>();
		}
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_uiso = FindAnyObjectByType<UINGOTestSOHolder>().Data;
		_uiso.Notification = FindAnyObjectByType<UINotification>();

		if (_playerInfo == null
			|| _tcpClient == null
			|| _uiso == null)
		{
			throw new Exception("Where is the SO holder in ngo scene");
		}

		var nm = NetworkManager.Singleton;
		var ut = nm.GetComponent<UnityTransport>();
		
		nm.OnServerStarted += OnServerStarted;
		nm.OnServerStopped += OnServerStopped;
		nm.OnClientStarted += OnClientStarted;
		nm.OnClientStopped += OnClientStopped;
		nm.OnConnectionEvent += OnConnectionEvent;
		nm.OnPreShutdown += OnPreShutdown;

		_uiso.OnClick_1 += () => { };
		_uiso.OnClick_2 += () => { };
		_uiso.OnClick_3 += () => {
			LeaveFromLobby();
		};
		_uiso.OnClick_4 += () => {
			_playerSpawner.Spawn();
		};
		_uiso.OnClick_5 += () => {
			_playerSpawner.Despawn();
		};
		_uiso.OnClick_6 += OnClickShowStatus;
		_tcpClient.OnReceived += OnTCPDataReceived;

		//if (!_playerInfoHolder.Instance.Debugging)
		{
			if (_playerInfo.Host)
			{
				_isHost = true;

				StartHost().Forget();
			}
			else
			{
				//_ = StartClient(_playerInfo.LobbyIdForEntry);
				StartCoroutine(StartClientCo(_playerInfo.LobbyIdForEntry));
			}
		}

		return;
	}

	void OnDestroy()
	{
		if (_heartbeatCoroutin != null)
		{
			StopCoroutine(_heartbeatCoroutin);
		}
	}

	void OnServerStarted() => GLogger.LogWarning($"NetworkManager OnServerStarted.");
	void OnServerStopped(bool isHost) => GLogger.LogWarning($"NetworkManager OnServerStopped. isHost: {isHost}");
	void OnClientStarted() => GLogger.LogWarning($"NetworkManager OnClientStarted. {NetworkManager.Singleton.LocalTime.Time} {NetworkManager.Singleton.ServerTime.Time}");
	void OnClientStopped(bool isHost) => Debug.LogWarning($"NetworkManager OnClientStopped. isHost: {isHost}");

	void OnConnectionEvent(NetworkManager nm, ConnectionEventData ced)
	{
		string eventLog = $"CE {ced.ClientId}";
		switch (ced.EventType)
		{
			case ConnectionEvent.ClientConnected: // This event is set on the client-side of the newly connected client and on the server-side.
				eventLog += "ClientConnected";
				break;
			case ConnectionEvent.ClientDisconnected: // This event is set on the client-side of the client that disconnected client and on the server-side.
				eventLog += "ClientDisconnected";
				break;
			case ConnectionEvent.PeerConnected: // This event is set on clients that are already connected to the session.
				eventLog += "PeerConnected";
				break;
			case ConnectionEvent.PeerDisconnected: // This event is set on clients that are already connected to the session.
				eventLog += "PeerDisconnected";
				break;
		}

		eventLog += '\n';

		eventLog += "--- Peer Client Ids ---\n";
		foreach (var peer in ced.PeerClientIds)
		{
			eventLog += $"{peer}\n";
		}
		GLogger.LogWarning(eventLog);
	}

	void OnPreShutdown() => GLogger.LogWarning("OnPreShutdown");

	async Awaitable StartHost()
	{
		Debug.Log($"START HOST max peer connection: {_maxPeerConnection}");
		//NetworkManager.Singleton.ConnectionApprovalCallback = (request, response) => {
		//	GLogger.Log($"ConnectionApprovalCallback request client id: {request.ClientNetworkId}");
		//	var reqClientId = request.ClientNetworkId;
			
		//	if (NetworkManager.Singleton.ConnectedClientsIds.Count >= _maxPeerConnection + 1)
		//	{
		//		GLogger.LogWarning($"Decline the peer connection request.(connected: {NetworkManager.Singleton.ConnectedClientsIds.Count})");
		//		response.Approved = false;
		//	}
		//	else
		//	{
		//		GLogger.LogWarning($"Approved the peer connection request.(connected: {NetworkManager.Singleton.ConnectedClientsIds.Count})");
		//		response.Approved = true;
		//	}
		//};
		
		// relay 최대 연결수 (host.에게 연결될 수 있는 client 수)
		(var successRelay, var joinCode) = await UGSRelayManager.StartHostWithRelayAndGetJoinCode(_maxPeerConnection - 1, "dtls");
		
		if (successRelay)
		{
			var callbacks = new LobbyEventCallbacks();
			callbacks.LobbyChanged += (changes) =>
			{
				GLogger.LogWarning($"LobbyChanged");
			};
			callbacks.KickedFromLobby += () =>
			{
				GLogger.LogWarning("KickedFromLobby");
			};
			
			callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;
			
			
			(var successLobby, var lobby, var lobbyEvents) = await UGSLobbyManager.CreateLobby(
				_playerInfo.LobbyName,
				_maxPeerConnection + 1, // lobby 최대 인원(host + client)
				joinCode,
				callbacks,
				_playerInfo.LobbyPassword);

			if (successLobby)
			{
				_lobby = lobby;
				_lobbyEvents = lobbyEvents;

				_heartbeatCoroutin = StartCoroutine(HeartbeatLobby());
			}
			else
			{
				GLogger.LogError("Lobby 생성 실패");
				LoadScene("LobbyScene");
			}
		}
		else
		{
			GLogger.LogError("Host 시작 실패");
			LoadScene("LobbyScene");
		}

		return;
	}



	async Task StartClient(string lobbyId, string password = null)
	{
		Debug.LogWarning("Start CLIENT");

		(var lobby, var reason) = await UGSLobbyManager.JoinLobbyById(
			lobbyId, 
			password);
		
		if (lobby != null)
		{
#if UNITY_EDITOR
			Debug.Log("Join Lobby Success!!!\n"
				+ $"Player ID: {AuthenticationService.Instance.PlayerId}\n"
				+ $"	{lobby.Id}\n"
				+ $"	{lobby.Name}\n"
				+ $"	{lobby.Created}\n"
				+ $"	{lobby.HostId}\n"
				+ $"	{lobby.Data["GameMode"].Value}\n"
				+ $"	{lobby.Data["GamePlaying"].Value}\n"
				+ $"	{lobby.Data["RelayJoinCode"].Value}\n"
				);
#endif
			
			_lobby = lobby;

			await UGSRelayManager.StartClientWithRelay(lobby.Data["RelayJoinCode"].Value, "dtls");
			await Awaitable.MainThreadAsync();
			//Spawn();
		}
		else
		{
			GLogger.LogError("StartClient failed to join lobby. reason: {reason}");
			LoadScene("NGOTestScene");
		}

		return;
	}

	IEnumerator StartClientCo(string lobbyId, string password = null)
	{
		Debug.LogWarning("START CLIENT COROUTINE");
		//NetworkManager.Singleton.ConnectionApprovalCallback = (request, response) => {
		//	GLogger.Log($"ConnectionApprovalCallback request client id: {request.ClientNetworkId}");
		//	var reqClientId = request.ClientNetworkId;

		//	if (NetworkManager.Singleton.ConnectedClientsIds.Count >= _maxPeerConnection + 1)
		//	{
		//		GLogger.LogWarning($"Client: Decline the peer connection request.(connected: {NetworkManager.Singleton.ConnectedClientsIds.Count})");
		//		response.Approved = false;
		//	}
		//	else
		//	{
		//		GLogger.LogWarning($"Client: Approved the peer connection request.(connected: {NetworkManager.Singleton.ConnectedClientsIds.Count})");
		//		response.Approved = true;
		//	}
		//};
		var joiningTask = UGSLobbyManager.JoinLobbyById(lobbyId, password);

		while (!joiningTask.IsCompleted)
		{
			yield return new WaitForSeconds(0.02f);
		}

		(var lobby, var reason) = joiningTask.Result;

		if (lobby != null)
		{
#if UNITY_EDITOR
			Debug.Log("Join Lobby Success!!!\n"
				+ $"Player ID: {AuthenticationService.Instance.PlayerId}\n"
				+ $"	{lobby.Id}\n"
				+ $"	{lobby.Name}\n"
				+ $"	{lobby.Created}\n"
				+ $"	{lobby.HostId}\n"
				+ $"	{lobby.Data["GameMode"].Value}\n"
				+ $"	{lobby.Data["GamePlaying"].Value}\n"
				+ $"	{lobby.Data["RelayJoinCode"].Value}\n"
				);
#endif

			_lobby = lobby;

			var startingRelayTask =  UGSRelayManager.StartClientWithRelay(lobby.Data["RelayJoinCode"].Value, "dtls");
			while (!startingRelayTask.IsCompleted)
			{
				yield return new WaitForSeconds(0.02f);
			}
			//Spawn();
		}
		else
		{
			GLogger.LogError($"StartClientCo JoinLobbyById {reason}");
			LoadScene("NGOTestScene");
		}

		yield break;	}

	async void OnClientStopped()
	{
		var (result, lobby) = await UGSLobbyManager.GetLobbyById(_lobby.Id);
		if (result)
		{
			_lobby = lobby;

			Debug.LogWarning($"NGOGameManager.OnClientStopped new host id: {_lobby.HostId} {AuthenticationService.Instance.PlayerId}");
		}
	}
	IEnumerator HeartbeatLobby()
	{
		while (true) {
			yield return new WaitForSeconds(20f);

			if (_lobby == null)
			{
				Debug.LogWarning("NGOGameManager.HeartbeatLobby lobby is null");
				continue;
			}

			_ = UGSLobbyManager.MaintainLobbyAlive(_lobby);
		}
	}


	void OnLobbyEventConnectionStateChanged(LobbyEventConnectionState state)
	{
		switch (state)
		{
			case LobbyEventConnectionState.Unsubscribed:
				/* Update the UI if necessary, as the subscription has been stopped. */
				Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Unsubscribed");
				break;
			case LobbyEventConnectionState.Subscribing:
				/* Update the UI if necessary, while waiting to be subscribed. */
				Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Subscribing");
				break;
			case LobbyEventConnectionState.Subscribed:
				/* Update the UI if necessary, to show subscription is working. */
				Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Subscribed");
				break;
			case LobbyEventConnectionState.Unsynced:
				/* Update the UI to show connection problems. Lobby will attempt to reconnect automatically. */
				Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Unsynced");
				break;
			case LobbyEventConnectionState.Error:
				/* Update the UI to show the connection has errored. Lobby will not attempt to reconnect as something has gone wrong. */
				Debug.LogWarning("NGOGameManager.OnLobbyEventConnectionStateChanged Error");
				break;
		}
	}

	async void LeaveFromLobby()
	{
		Debug.LogWarning("NGOGameManager.LeaveFromLobby");
		await UGSLobbyManager.RemovePlayer(_lobby);
		NetworkManager.Singleton.Shutdown();
		LoadScene("LobbyScene");
	}

	async void OnClickShowStatus()
	{
		if (_lobby == null)
		{
			return;
		}

		await UGSLobbyManager.GetLobbyById(_lobby.Id);

		var nm = NetworkManager.Singleton;
		NetworkClient nc = new();
		//_uiso.SetText(
		string status = $"=== Network Manager Info ===\n" +
			$"AS player id: {AuthenticationService.Instance.PlayerId}\n" +
			$"local client id: {nm.LocalClientId}\n" +
			$"is host: {nm.IsHost}\n" +
			$"is client: {nm.IsClient}\n" +
			$"is connected client: {nm.IsConnectedClient}\n" + // 서버(host)에 연결되고 승인되고 동기화되고 있는가
			$"is active and enabled: {nm.isActiveAndEnabled}\n" +
			$"is approved: {nm.IsApproved}\n" +
			$"is listening: {nm.IsListening}\n" +
			$"session name: {_sessionName}\n" +
			$"joinCode: {_joinCode}\n" +
			$"Connected host name: {nm.ConnectedHostname}\n" +
			$"Current session owner: {nm.CurrentSessionOwner}\n";
		
		status += "=== Connected Clients ===\n";
		foreach (var id in nm.ConnectedClientsIds)
		{
			status += $"{id}\n";
		}

		status += "\n";
		status += UGSLobbyManager.LobbyInfo(_lobby);

		_uiso.ShowText(status);

		Debug.LogWarning(status);
	}

	async Task OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			//_playerInfo.MessageFromPreviousScene = "Disconnected from the server.";
			GLogger.LogError("NGOGameManager.OnTCPDataReceivec Disconnect from server.");
			SceneManager.LoadScene("LoginScene");

			return;
		}

		SessionMessage_Type type = (SessionMessage_Type)BitConverter.ToInt32(buffer, 4);

		Debug.Log($"OnDataReceivecFromServer({type.ToString()}, data, {length}");

		if (type == SessionMessage_Type.Environment)
		{
			M_Environment env;

			try
			{
				env = M_Environment.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogError($"M_Environment ParseFrom error: {e.Message}");
				return;

			}

			_isHost = env.Host;
			_sessionName = env.SessionName;
			_joinCode = env.JoinCode;
			_password = env.Password;

			await Awaitable.MainThreadAsync();

			//if (_isHost)
			//{
			//	StartHost();
			//}
			//else
			//{
			//	StartClient(_joinCode);
			//}
		}

	}

	void LoadScene(string sceneName)
	{
		_uiso.ClearEvent();
		var nm = NetworkManager.Singleton;
		nm.OnServerStarted -= OnServerStarted;
		nm.OnServerStopped -= OnServerStopped;
		nm.OnClientStarted -= OnClientStarted;
		nm.OnClientStopped -= OnClientStopped;
		nm.OnConnectionEvent -= OnConnectionEvent;
		nm.OnPreShutdown -= OnPreShutdown;
		_tcpClient.OnReceived -= OnTCPDataReceived;
		SceneManager.LoadScene(sceneName);
	}
}
