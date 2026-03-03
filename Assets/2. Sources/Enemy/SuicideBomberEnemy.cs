using JetBrains.Annotations;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;



public class SuicideBomberEnemy : EnemyBase, INetworkObjectCollision
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	Collider2D _colliderTrigger;
	[SerializeField]
	NavMeshAgent _navMeshAgent;

	IPooledDynamicSpawner _pds;
	List<CollisionEvent> _collisionEventList;
	CollisionEventStruct _collisionEventCache;
	//NetworkVariable<CollisionEventStruct> _collisionEventCache = new(
	//	new()
	//	{
	//		Damage = 20,
	//		Effect = CollisionEffect.Suicide,
	//	},
	//	NetworkVariableReadPermission.Everyone,
	//	NetworkVariableWritePermission.Server);

	public override void OnNetworkSpawn()
	{
		if (!IsHost)
		{
			return;
		}

		base.OnNetworkSpawn();
		_collisionEventList = new();
		_collisionEventCache = new()
		{
			SenderId = NetworkObjectId,
			Damage = 20,
			Effect = CollisionEffect.Suicide,
		};
		HealthPoint = 100;
		_colliderTrigger.gameObject.SetActive(true);
		_navMeshAgent.updateRotation = false;
		_navMeshAgent.updateUpAxis = false;
	}

	void FixedUpdate()
	{
		if (!IsHost)
		{
			return;
		}

		while (_collisionEventList.Count > 0)
		{
			var ce = _collisionEventList.First();
			_collisionEventList.RemoveAt(0);

			if (ce.Effect == CollisionEffect.Knockback)
			{
				HealthPoint -= ce.Damage;

				if (HealthPoint == 0)
				{
					DespawnEnemyRpc();
					return;
				}
				else
				{
					ApplyKnockback(
						ce.Direction,
						ce.EffectIntensity,
						ce.EffectDuration);
				}
			}
			else if (ce.Effect == CollisionEffect.Block)
			{
				// player와 충돌함
				if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(ce.SenderId, out var obj))
				{
					var pp = obj.GetComponent<PointmanPlayer>();
					pp.SendCollisionEvent(
						new CollisionEvent().FromCollisionEventStruct(_collisionEventCache));
				}

				_collisionEventList.Clear();
				DespawnEnemyRpc();
				return;
			}
		}

		if (IsEffectInProgress)
		{
			_navMeshAgent.isStopped = true;
		}
		else
		{
			if (Target != null)
			{
				_navMeshAgent.isStopped = false;
				_navMeshAgent.SetDestination(Target.position);
			}
		}
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
