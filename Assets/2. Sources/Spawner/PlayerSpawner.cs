using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/*
 * in-scene placed 객체로 있어야 함
 * player prefab을 spawn
 */
public class PlayerSpawner : NetworkBehaviour, IPlayerSpawner, IPlayerSpawnObserver
{
	[SerializeField]
	GameObject _prefapToSpawn;
	[SerializeField]
	InputSystem _inputSystem;
	[SerializeField]
	PooledDynamicSpawner _pooledDynamicSpawner;

	SpawnPlayerWithDataHandler _spawnHandler;
	Dictionary<ulong, IPlayerHandler> _activedPlayer;

	public event UnityAction<IPlayerHandler> PlayerSpawned;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_spawnHandler = new SpawnPlayerWithDataHandler(
			networkManager, 
			_prefapToSpawn,
			this);

		if (networkManager.IsHost)
		{
			_activedPlayer = new();
			//_spawnHandler.PlayerObjectInstantiated += OnPlayerObjectInstantiated;
			_spawnHandler.PlayerObjectDestroyed += OnPlayerObjectDestroyed;
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

		_activedPlayer[pp.NO.NetworkObjectId] = pp;
	}

	[Rpc(SendTo.Server)]
	public void DespawnPlayerRpc(RpcParams rpcParam = default)
	{
		_spawnHandler.InactiveAndDespawn(rpcParam.Receive.SenderClientId);
	}

	void OnPlayerObjectDestroyed(NetworkObject networkObject)
	{
		_activedPlayer.Remove(networkObject.NetworkObjectId);
	}

	public List<IPlayerHandler> GetPlayers()
	{
		return _activedPlayer.Values.ToList();
	}

	public void NotifyPlayerSpawned(IPlayerHandler ph)
	{
		PlayerSpawned?.Invoke(ph);
	}
}
