using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour, ICollisionInteractable
{
	public void AddCollisionEvent(CollisionEvent ce)
	{
		
	}

	[Rpc(SendTo.Server)]
	void HitWallRpc(RpcParams rpcParams = default)
	{
		GLogger.Log($"{rpcParams.Receive.SenderClientId} hit wall");
	}

	public CollisionEffect GetEffect()
	{
		return CollisionEffect.None;
	}
}
