using NUnit.Framework;
using Unity.Netcode;
using UnityEngine;

public interface IEnemyHandler
{
	public string PrefabId { get; set; }
	public NetworkObject NO { get; }
	public GameObject GO { get; }
	public IEnemySpawner Spawner { get; set; }
	public Transform Target { get; set; }
	public void DespawnEnemyRpc();
	public void SetData(in EnemyInstantiateData data);
}

public interface IEnemySpawner
{
	public void SpawnEnemyRpc(
		Vector2 spawnPosition,
		Quaternion rotation,
		EnemyInstantiateData data,
		RpcParams rpcParam = default);
}