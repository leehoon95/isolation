using Google.Protobuf;
using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NGOGameManager : NetworkBehaviour
{
	[SerializeField]
	TCPClientSO _tcpClient;
	[SerializeField]
	UINGOTestSO _uiso;
	[SerializeField]
	NetworkObject _character;
	[SerializeField]
	PlayerInfoSO _userInfo;

	PlayerInfoHolder _PlayerInfoHolder;
	int _pingPongCount;

	bool _isHost;
	string _sessionName;
	string _joinCode;
	string _password;

	void Start()
	{
		var nm = NetworkManager.Singleton;

		nm.OnClientConnectedCallback += (ulong id) =>
		{
			Debug.LogWarning($"NetworkManager {nm.LocalClientId} OnClientConnectedCallback. id: {id}");
			_ = Spawn();
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

		_PlayerInfoHolder = FindAnyObjectByType<PlayerInfoHolder>();
		if (_PlayerInfoHolder == null)
		{
			throw new NullReferenceException("Loading UserInfo failed");
		}

		//_uiso.OnClickStartHost += StartHost;
		//_uiso.OnClickStartClient += StartClient;
		_uiso.OnClickShutdown += Shutdown;
		_uiso.OnClickSpawn += Spawn;
		_uiso.OnClickShowStatus += OnClickShowStatus;
		_tcpClient.OnReceived += OnTCPDataReceived;

		if (_PlayerInfoHolder.PlayerInfo.StartHost)
		{
			_isHost = true;
		}

		return;

		M_RequestEnvironment re = new();
		re.Filter = 1;
		var data = re.ToByteArray();
		_ = _tcpClient.SendDataAsync((int)SessionMessage_Type.RequestEnvironment, data);
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		//_uiso.OnClickStartHost -= StartHost;
		//_uiso.OnClickStartClient -= StartClient;
		_uiso.OnClickShutdown -= Shutdown;
		_uiso.OnClickSpawn -= Spawn;
		_uiso.OnClickShowStatus -= OnClickShowStatus;
		_tcpClient.OnReceived -= OnTCPDataReceived;
	}

	async void StartHost()
	{
		var res = await UGSRelayManager.StartHostWithRelayAndGetJoinCode(1, "dtls");

		if (res.Item1)
		{
			Debug.Log($"START HOST\n" +
				$"result: {res.Item1}, joinCode: {res.Item2}");
		}
		else
		{
			Debug.Log($"START HOST \n" +
				$"fail reason: {res.Item2}");

			return;
		}

		M_JoinCode j = new();

		j.JoinCode = res.Item2;

		var data = j.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)SessionMessage_Type.Joincode, data); // activate session
	}
	async void StartClient(string joinCode)
	{
		Debug.LogWarning("Start CLIENT");
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

	async Awaitable Spawn()
	{
		var nm = NetworkManager.Singleton;

		await Awaitable.MainThreadAsync();

		if (nm.IsHost)
		{

			var obj = Instantiate(_character, default, Quaternion.identity);

			obj.GetComponent<NetworkObject>()
				.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId, true);
		}
		else
		{
			SpawnObjectRpc();
		}
	}

	void OnClickShowStatus()
	{
		var nm = NetworkManager.Singleton;
		NetworkClient nc = new();
		//_uiso.SetText(
		string status = $"===Network Manager Status===\n" +
			$"local client id: {nm.LocalClientId}\n" +
			$"is server: {nm.IsServer}\n" +
			$"is host: {nm.IsHost}\n" +
			$"is client: {nm.IsClient}\n" +
			$"is connected client: {nm.IsConnectedClient}\n" +
			$"is active and enabled: {nm.isActiveAndEnabled}\n" +
			$"is approved: {nm.IsApproved}\n" +
			$"is listening: {nm.IsListening}\n" +
			$"session name: {_sessionName}\n" +
			$"joinCode: {_joinCode}\n" +
			$"Connected host name: {nm.ConnectedHostname}\n" +
			$"Current session owner: {nm.CurrentSessionOwner}\n";

		status += "===Connected Client IDs===\n";
		foreach (var id in nm.ConnectedClientsIds)
		{
			status += $"	{id}\n";
		}

		status += "===Connected clients===\n";
		foreach (var c in nm.ConnectedClients)
		{
			status += $"	{c.Key} {c.Value}\n";
		}

		Debug.Log(status);
	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			_PlayerInfoHolder.PlayerInfo.MessageFromPreviousScene = "Disconnected from the server.";
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

			if (_isHost)
			{
				StartHost();
			}
			else
			{
				StartClient(_joinCode);
			}
		}

	}

	void OnClickPingPong() => PingRpc(_pingPongCount++, NetworkManager.Singleton.RpcTarget.Server);



	[Rpc(SendTo.Server)]
	void SpawnObjectRpc(RpcParams rpcParams = default)
	{
		print($"SpawnObjejctRpc() called. Sender client id: {rpcParams.Receive.SenderClientId}");

		//if (!NetworkManager.Singleton.IsHost)
		//{
		//	return;
		//}

		var obj = Instantiate(_character, default, Quaternion.identity);

		obj.GetComponent<NetworkObject>()
			.SpawnAsPlayerObject(rpcParams.Receive.SenderClientId, true);

		print("Spawn complete.");
	}

	[Rpc(SendTo.Server)]
	void PingRpc(int pingCount, RpcParams rpcParams)
	{
		print($"Received ping. message: {pingCount}");

		PongRpc(
			pingCount,
			"Pong!",
			NetworkManager.Singleton.RpcTarget.Single(
				rpcParams.Receive.SenderClientId,
				RpcTargetUse.Temp)
			);
	}

	[Rpc(SendTo.SpecifiedInParams)]
	void PongRpc(int pingCount, string message, RpcParams rpcParams)
	{
		print($"Received pong. ping count: {pingCount}, message: {message}");
	}
}
