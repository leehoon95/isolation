using Unity.Netcode;
using UnityEngine;

public class Wall : NetworkBehaviour, INetworkObjectCollision
{
	CollisionEvent _collisionEventCache = new()
	{
		SenderId = 0,
		Effect = CollisionEffect.None,
	};

	public void SendCollisionEvent(CollisionEvent ce)
	{
	}

	public CollisionEvent GetCollisionEvent()
	{
		return _collisionEventCache;
	}
}
