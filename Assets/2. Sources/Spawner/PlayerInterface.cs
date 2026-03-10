using Unity.Netcode;
using UnityEngine;

public interface IPlayerHandler
{
	public NetworkObject NO { get; }
	public GameObject GO { get; }
	public string Nickname { set; }
	public Color PersonalColor { set; }
	public IPlayerSpawnObserver SpawnObserver { set; }
	public Transform CameraTarget { get; }
}

public interface IPlayerSpawner
{
	public void SpawnPlayerRpc(
	Vector2 spawnPosition,
	Quaternion rotation,
	PlayerInstantiateData data,
	RpcParams rpcParam = default);

	public void DespawnPlayerRpc(RpcParams rpcParam = default);
}

public interface IPlayerSpawnObserver
{
	public void NotifyPlayerSpawned(IPlayerHandler ph);
}