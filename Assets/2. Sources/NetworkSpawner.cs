using JetBrains.Annotations;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class NetworkSpawner : NetworkBehaviour
{
	[SerializeField]
	public GameObject _targetPrefab;

	/*
	 * 
	 */
	public void SpawnPrefab()
	{
		if (IsHost)
		{
			SpawnObject();
		}
		else
		{
			SpawnObjectRpc();
		}
	}

	public void SpawnPrefabWithOwnership()
	{
		if (IsHost)
		{
			SpawnObjectWithOwnership(NetworkObjectId);
		}
		else
		{
			SpawnObjectWithOwnershipRpc(NetworkObjectId);
		}
	}

	public void SpawnNetworkBehaviour(GameObject go)
	{
		var no = go.GetComponent<NetworkBehaviour>();
		if (no == null)
		{
			return;
		}

		if (IsHost)
		{
			SpawnNetworkObject(no);
		}
		else
		{
			SpawnObjectRpc(no);
		}
	}

	[Rpc(SendTo.Server)]
	void SpawnObjectRpc(RpcParams rpcParams = default)
	{
		Debug.Log($"NetworkSpawner.RequestSpawnObjectServerRpc() called. Sender client id: {rpcParams.Receive.SenderClientId}");


		SpawnObject();
	}

	[Rpc(SendTo.Server)]
	void SpawnObjectWithOwnershipRpc(ulong clientId, RpcParams rpcParams = default)
	{
		Debug.Log($"NetworkSpawner.RequestSpawnObjectWithOwnershipServerRpc() called. Sender client id: {rpcParams.Receive.SenderClientId}");

		SpawnObjectWithOwnership(clientId);
	}

	[Rpc(SendTo.Server)]
	void SpawnObjectRpc(NetworkBehaviourReference nbr, RpcParams rpcParams = default)
	{
		Debug.Log($"NetworkSpawner.RequestSpawnObjectServerRpc() called. Sender client id: {rpcParams.Receive.SenderClientId}");


		SpawnNetworkObject(nbr);
	}

	/*
	 * 명시적으로 owner id를 지정하지 않을 경우 authority가 owner가 된다
	 * (Server-Client 권한 모델에서는 Server가 authority를 가진다)
	 */
	void SpawnObject()
	{
		var obj = Instantiate(_targetPrefab);
		obj.GetComponent<NetworkObject>().Spawn(true);
	}

	void SpawnObjectWithOwnership(ulong clientId)
	{
		var obj = Instantiate(_targetPrefab);
		obj.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkObjectId, true);
	}

	void SpawnNetworkObject(NetworkBehaviourReference nbr)
	{
		if (nbr.TryGet(out NetworkBehaviour nb))
		{
			var obj = Instantiate(nb.GameObject());
			obj.GetComponent<NetworkObject>().Spawn(true);
		}
		
	}

	//void SpawnWithOwnership(GameObject go)
	//{
	//	if (withOwnership)
	//	{
	//		/*
	//	 * RpcParams.Receive: Sender's identifier
	//	 * RpcParams.Send: 
	//	 */
	//		go.GetComponent<NetworkObject>()
	//			.SpawnWithOwnership(NetworkObjectId, true);
	//	}
	//	else
	//	{
	//		go.GetComponent<NetworkObject>().Spawn();
	//	}
	//}

	[Rpc(SendTo.Server, RequireOwnership = false)]
	void PingRpc(int pingCount, RpcParams rpcParams)
	{
		Debug.Log($"Received ping. message: {pingCount}");

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
		Debug.Log($"Sent PONG to {rpcParams.Receive.SenderClientId}. ping count: {pingCount}, message: {message}");
	}
}
