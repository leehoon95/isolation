using Google.Protobuf;
using System;
using System.Collections;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NGOGameManager : MonoBehaviour
{
	[SerializeField]
	GameObject _targetPrefab;
	[SerializeField]
	NetworkSpawner _spawner;

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
		_playerInfo = FindAnyObjectByType<PlayerInfoHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientHolder>().Data;
		_uiso = FindAnyObjectByType<UINGOTestSOHolder>().Data;
		_uiso.Notification = FindAnyObjectByType<UINotification>();

		if (_playerInfo == null
			|| _tcpClient == null
			|| _uiso == null)
		{
			throw new Exception("Where is the SO holder in ngo scene");
		}

		var nm = NetworkManager.Singleton;

		
		nm.OnClientConnectedCallback += (ulong id) =>
		{
			Debug.LogWarning($"NetworkManager {nm.LocalClientId} OnClientConnectedCallback. id: {id}");
			//_ = Spawn();
		};

		nm.OnClientStarted += () =>
		{
			Debug.LogWarning($"NetworkManager {nm.LocalClientId} OnClientStarted.");

		};

		nm.OnClientDisconnectCallback += (ulong id) =>
		{
			Debug.LogWarning($"NetworkManager {nm.LocalClientId} OnClientDisconnectCallback. id: {id}");
		};

		nm.OnClientStopped += (bool isHost) =>
		{
			Debug.LogWarning($"NetworkManager {nm.LocalClientId} OnClientStopped. isHost: {isHost}");
		};
		

		_uiso.OnClick_1 += () => { };
		_uiso.OnClick_2 += () => { };
		//_uiso.OnClick_3 += () => { };
		_uiso.OnClick_4 += () => {
			if (nm.IsHost)
			{
				_spawner.SpawnPrefab();
			}
			else
			{
				_spawner.SpawnPrefabWithOwnership();
			}
		};
		_uiso.OnClick_5 += () => {
			if (nm.IsHost)
			{
				_spawner.SpawnPrefab();
			}
			else
			{
				_spawner.SpawnPrefabWithOwnership();
			}
		};
		_uiso.OnClick_6 += OnClickShowStatus;
		_tcpClient.OnReceived += OnTCPDataReceived;

		//if (!_playerInfoHolder.Instance.Debugging)
		{
			if (_playerInfo.StartHost)
			{
				_isHost = true;

				StartHost().Forget();
			}
			else
			{
				StartClient(_playerInfo.LobbyIdForEntry).Forget();
			}
		}

		return;

		M_RequestEnvironment re = new();
		re.Filter = 1;
		var data = re.ToByteArray();
		_ = _tcpClient.SendDataAsync((int)SessionMessage_Type.RequestEnvironment, data);
	}

	void OnDestroy()
	{
		_uiso.ClearEvent();

		if (_heartbeatCoroutin != null)
		{
			StopCoroutine(_heartbeatCoroutin);
		}
	}

	async Awaitable StartHost()
	{
		Debug.Log($"START HOST");
		(var successRelay, var joinCode) = await UGSRelayManager.StartHostWithRelayAndGetJoinCode(1, "dtls");

		if (successRelay)
		{
			var callbacks = new LobbyEventCallbacks();
			callbacks.LobbyChanged += OnLobbyChanged;
			callbacks.KickedFromLobby += OnKickedFromLobby;
			callbacks.LobbyEventConnectionStateChanged += OnLobbyEventConnectionStateChanged;

			(var successLobby, var lobby, var lobbyEvents) = await UGSLobbyManager.CreateLobby(
				_playerInfo.LobbyName,
				2,
				joinCode,
				callbacks,
				_playerInfo.LobbyPassword);

			if (successLobby)
			{
				_lobby = lobby;
				_lobbyEvents = lobbyEvents;

				//Spawn();

				StartCoroutine(HeartbeatLobby());
			}
			else
			{
				_playerInfo.MessageFromPreviousScene = "Lobby 생성 실패";

				SceneManager.LoadScene("LobbyScene");
			}
		}
		else
		{
			_playerInfo.MessageFromPreviousScene = "Host 시작 실패";

			SceneManager.LoadScene("LobbyScene");
		}

		return;

		M_JoinCode j = new();

		j.JoinCode = joinCode;

		var data = j.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)SessionMessage_Type.Joincode, data); // activate session
	}

	async Awaitable StartClient(string joinCode, string password = null)
	{
		Debug.LogWarning("Start CLIENT");

		(var lobby, var reason) = await UGSLobbyManager.JoinLobbyById(
			joinCode, 
			password);

		if (lobby != null)
		{
#if UNITY_EDITOR
			Debug.Log("Join Lobby Success!!!\n"
				+ $"Player ID: {AuthenticationService.Instance.PlayerId}"
				+ $"	{lobby.Id}"
				+ $"	{lobby.Name}"
				+ $"	{lobby.Created}"
				+ $"	{lobby.HostId}"
				+ $"	{lobby.Data["GameMode"].Value}"
				+ $"	{lobby.Data["GamePlaying"].Value}"
				+ $"	{lobby.Data["RelayJoinCode"].Value}"
				);

#endif

			_lobby = lobby;

			await UGSRelayManager.StartClientWithRelay(lobby.Data["RelayJoinCode"].Value, "dtls");

			//Spawn();
		}
		else
		{
			if (reason == LobbyExceptionReason.LobbyFull)
			{
				_playerInfo.MessageFromPreviousScene = "Session is full";
			}
			else
			{
				_playerInfo.MessageFromPreviousScene = "세션에 들어갈 수 없습니다";
			}

			SceneManager.LoadScene("NGOTestScene");
		}

		return;
		try
		{
			await UGSRelayManager.StartClientWithRelay(joinCode, "dtls");
		}
		catch (Exception e)
		{
			Debug.LogError($"외부 로그 {e.Message}");
		}

		Debug.LogWarning("Start CLIENT END");
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

	void OnLobbyChanged(ILobbyChanges changes)
	{
		if (changes.LobbyDeleted)
		{
			Debug.LogWarning("NGOGameManager.OnLobbyChanged LobbyDeleted");
		}
		else
		{
			changes.ApplyToLobby(_lobby);
		}
	}

	void OnKickedFromLobby()
	{
		Debug.LogWarning("NGOGameManager.OnKickedFromLobby Kicked");
		_lobbyEvents = null;
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

	void Shutdown()
	{
		var nm = NetworkManager.Singleton;

		//if (nm.IsHost)
		{
			// client에 전파
			nm.Shutdown();

			SceneManager.LoadScene("LoginScene");
		}
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

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			_playerInfo.MessageFromPreviousScene = "Disconnected from the server.";
			_ = SceneManager.LoadSceneAsync("LoginScene");

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
}
