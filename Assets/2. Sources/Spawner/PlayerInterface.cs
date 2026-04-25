using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public interface IPlayerHandler
{
	public NetworkObject NO { get; }
	public GameObject GO { get; }
	public string Nickname { get; set; }
	public Color PersonalColor { set; }
	public IPlayerSpawner Spawner { get; set; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public InputSystem InputSystem { set; get; }
	public Transform CameraTarget { get; }
	// owner가 되는 client의 id(소유권이 server 상태로 spawn하고 client으로 변경됨)
	public ulong SpawnClientId { get; set; } 
	public bool AutomaticMotion { get; set; }
}

public interface IPlayerDeadBodyHandler
{
	public NetworkObject NO { get; }
	public GameObject GO { get; }
	public string Nickname { get; set; }
	public Color PersonalColor { get; set; }
	public IPlayerSpawner Spawner { get; set; }
	public ulong ClientIdForRevive { get; set; }
}

public interface IPlayerSpawner
{
	public void SpawnPlayerRpc(
		ulong ownerId,
		Vector2 spawnPosition,
		Quaternion rotation,
		PlayerInstantiateData data,
		RpcParams rpcParam = default);
	public void SpawnPlayerDeadBodyRpc(
		Vector2 spawnPosition,
		Quaternion rotation,
		FixedString32Bytes nickname,
		Color personalColor,
		RpcParams rpcParam = default);
	public IPlayerHandler GetPlayer(ulong id);
	public void DespawnPlayerRpc(RpcParams rpcParam = default);
	public void NotifyPlayerSpawned(IPlayerHandler ph);
	public void NotifyPlayerDespawned(IPlayerHandler ph);
}