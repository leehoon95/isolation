using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/*
 * in-scene placed 객체로 있어야 함
 * player prefab을 spawn
 */
public class PlayerSpawner : NetworkBehaviour, IPlayerSpawner
{
	[SerializeField]
	GameObject _prefapToSpawn;
	[SerializeField]
	InputSystem _inputSystem;
	[SerializeField]
	PooledDynamicSpawner _pooledDynamicSpawner;

	SpawnPlayerWithDataHandler _spawnHandler;

	Dictionary<ulong, IPlayerHandler> _activedPlayer;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_spawnHandler = new SpawnPlayerWithDataHandler(networkManager, _prefapToSpawn);
		if (IsHost)
		{
			_activedPlayer = new();
			_spawnHandler.NetworkObjectDestroyed += OnNetworkObjectDestroyed;
		}
	}

	/*
	 * host는 handler 메서드를 사용해서 바로 스폰한다.
	 * client side에서는 handler를 통해 prefab override를 진행한다
	 */
	[Rpc(SendTo.Server)]
	public void SpawnPlayerRpc(
		Vector2 spawnPosition,
		Quaternion rotation,
		PlayerInstantiateData data,
		RpcParams rpcParam = default)
	{
		var pp = _spawnHandler.InstantiateWithDataAndSpawn(
			rpcParam.Receive.SenderClientId,
			spawnPosition, rotation, data);

		_activedPlayer[pp.NetworkObjectId] = pp;
	}

	[Rpc(SendTo.Server)]
	public void DespawnPlayerRpc(RpcParams rpcParam = default)
	{
		_spawnHandler.InactiveAndDespawn(rpcParam.Receive.SenderClientId);
	}

	void OnNetworkObjectDestroyed(NetworkObject networkObject)
	{
		_activedPlayer.Remove(networkObject.NetworkObjectId);
	}

	public List<IPlayerHandler> GetPlayers()
	{
		return _activedPlayer.Values.ToList();
	}
}
