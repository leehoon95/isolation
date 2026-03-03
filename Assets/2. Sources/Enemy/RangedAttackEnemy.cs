using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RangedAttackEnemy : EnemyBase, INetworkObjectCollision
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	Collider2D _colliderTrigger;

	IPooledDynamicSpawner _pds;
	List<CollisionEvent> _collisionEventList;
	HashSet<ulong> _waitingForCollisionEventProcessing;

	CollisionEventStruct _collisionEventCache =
	new()
	{
		Effect = CollisionEffect.Block,
	};

	public override void OnNetworkSpawn()
	{
		if (!IsHost)
		{
			return;
		}

		base.OnNetworkSpawn();
		_collisionEventList = new();
		_waitingForCollisionEventProcessing = new();
		_collisionEventCache = new()
		{
			SenderId = NetworkObjectId,
			Damage = 20,
			Effect = CollisionEffect.Suicide,
		};
		HealthPoint = 100;
		_colliderTrigger.gameObject.SetActive(true);
	}

	[Rpc(SendTo.Server)]
	public void AddCollisionEventRpc(CollisionEventStruct ce)
	{
		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ce));
	}


	public void InvalidateUntilDespawn()
	{
		_colliderTrigger.gameObject.SetActive(false);
	}

	public void SendCollisionEvent(CollisionEvent ce)
	{
		AddCollisionEventRpc(ce);
	}

	public CollisionEvent GetCollisionEvent()
	{
		return new CollisionEvent().FromCollisionEventStruct(_collisionEventCache);
	}
}
