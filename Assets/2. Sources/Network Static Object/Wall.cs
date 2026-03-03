using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour, INetworkObjectCollision
{
	CollisionEvent _collisionEventCache;

	public override void OnNetworkSpawn()
	{
		_collisionEventCache = new()
		{ 
			SenderId = NetworkObjectId,
			Effect = CollisionEffect.Block,
		};

	}

	public void InvalidateUntilDespawn()
	{
	}

	public void SendCollisionEvent(CollisionEvent ce)
	{
	}

	public CollisionEvent GetCollisionEvent()
	{
		return _collisionEventCache;
	}
}
