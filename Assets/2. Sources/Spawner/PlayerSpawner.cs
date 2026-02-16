using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * in-scene placed 객체로 있어야 함
 * player prefab을 spawn
 */
public class PlayerSpawner : NetworkBehaviour
{
	[SerializeField]
	GameObject _prefapToSpawn;
	SpawnPlayerWithDataHandler _spawnHandler;

	Dictionary<ulong, PointmanPlayer> _data = new();

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_spawnHandler = new SpawnPlayerWithDataHandler(networkManager, _prefapToSpawn);
	}

	public override void OnNetworkSpawn()
	{
		var gm = FindAnyObjectByType<LevelGameManager>();
		gm.NotifyPlayerSpawnerSpawned(this);
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
		var no = _spawnHandler.InstantiateWithDataAndSpawn(
			rpcParam.Receive.SenderClientId,
			spawnPosition, rotation, data);
	}

	[Rpc(SendTo.Server)]
	public void DespawnPlayerRpc(RpcParams rpcParam = default)
	{
		_spawnHandler.InactiveAndDespawn(rpcParam.Receive.SenderClientId);
	}

	public void NotifyPlayerSpawned(PointmanPlayer player)
	{
		_data[player.OwnerClientId] = player;
	}

	public void NotifyPlayerDespawned(PointmanPlayer player)
	{
		_data.Remove(player.OwnerClientId);
	}
}
