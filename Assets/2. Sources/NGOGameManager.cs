using Google.Protobuf;
using Unity.Netcode;
using UnityEngine;

public class NGOGameManager : NetworkBehaviour
{
	[SerializeField]
	TCPClientSO _tcpClient;
	[SerializeField]
	UINGOTestSO _uiso;
	[SerializeField]
	NetworkObject _character;
	[SerializeField]
	UserInfoSO _userInfo;

	int _pingPongCount;

	void Start()
	{
		var nm = NetworkManager.Singleton;


		nm.OnClientConnectedCallback += (ulong id) =>
		{
			print($"OnClientConnectedCallback. id: {id}");
		};

		nm.OnClientStarted += () =>
		{
			print($"OnClientStarted.");
		};

		nm.OnClientDisconnectCallback += (ulong id) =>
		{
			print($"OnClientDisconnectCallback. id: {id}");
		};

		if (UserInfo)

		_uiso.OnClickStartHost += StartHost;
		_uiso.OnClickStartClient += StartClient;
		_uiso.OnClickShutdown += Shutdown;
		_uiso.OnClickSpawn += Spawn;
		_uiso.OnClickShowStatus += OnClickShowStatus;
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		_uiso.OnClickStartHost -= StartHost;
		_uiso.OnClickStartClient -= StartClient;
		_uiso.OnClickShutdown -= Shutdown;
		_uiso.OnClickSpawn -= Spawn;
		_uiso.OnClickShowStatus -= OnClickShowStatus;
	}

	async void StartHost()
	{
		var joincode = await RelayManager.StartHostWithRelayAndGetJoinCode(4, "dtls");
		_uiso.SetText(joincode);
		Debug.LogWarning($"Send to server the joinCode: {joincode}");

		if (_userInfo.Debugging)
		{
			return;
		}

		M_Joincode j = new();

		j.Joincode = joincode;

		var data = j.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)GameMessage_Type.Joincode, data);

		_uiso.ShowNotification($"Send a joincode : {joincode}");
	}
	async void StartClient()
	{


		await RelayManager.StartClientWithRelay("tempcode", "dtls");
	}

	void Shutdown()
	{
		var nm = NetworkManager.Singleton;

		if (nm.IsHost)
		{
			// client¿¡ ÀüÆÄ
			nm.Shutdown();
		}
	}

	void Spawn()
	{
		var nm = NetworkManager.Singleton;

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

		_uiso.SetText(
		$"local client id: {nm.LocalClientId}\n" +
		$"is server: {nm.IsServer}\n" +
		$"is host: {nm.IsHost}\n" +
		$"is client: {nm.IsClient}\n" +
		$"is connected client: {nm.IsConnectedClient}\n" +
		$"is active and enabled: {nm.isActiveAndEnabled}\n" +
		$"is approved: {nm.IsApproved}\n" +
		$"is listening: {nm.IsListening}\n"
		);
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
