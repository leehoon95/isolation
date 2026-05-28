using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Rendering;



public class SuicideBomberEnemy : EnemyBase, INetworkObjectCollision
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	Collider2D _colliderTrigger;
	[SerializeField]
	List<GameObject> _teeth;

	List<CollisionEvent> _collisionEventList;
	CollisionEventStruct _collisionEventCache;
	NavMeshPath _path;
	Coroutine _calculatePathCo;
	List<float> _teethSpeed = new();

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		_teethSpeed.Clear();
		for (int i = 0; i < _teeth.Count(); ++i)
		{
			_teethSpeed.Add(Random.Range(-60, 60));
		}

		if (!IsHost)
		{
			return;
		}
		
		HealthPoint = MaxHealthPoint;
		_collisionEventList = new();
		_collisionEventCache = new()
		{
			SenderId = 0,
			Damage = 20,
			Effect = CollisionEffect.Hit,
		};
		_path = new();

		_calculatePathCo = StartCoroutine(CalculatePathToTarget());
	}

	public override void OnNetworkDespawn()
	{
		base.OnNetworkDespawn();

		if (!IsHost)
		{
			return;
		}

		StopCoroutine(_calculatePathCo);
	}

	void Update()
	{
		for (int i = 0; i < _teeth.Count(); ++i)
		{
			_teeth[i].transform.RotateAround(transform.position, Vector3.forward, _teethSpeed[i] * Time.deltaTime);
		}
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
			HealthPoint -= ce.Damage;
			
			if (ce.Effect > CollisionEffect.None
				&& ce.Effect < CollisionEffect.Block)
			{
				var closestPoint = _colliderTrigger.ClosestPoint(ce.Position);
				var erp = new EffectRpcParameter()
				{
					EffectColor = Color.white
				};
				erp.Data.Append(ce.Damage);

				IPDS.CreateEffect(
					"EffectDamage",
					closestPoint,
					Quaternion.identity,
					erp);
			}

			if (HealthPoint == 0)
			{
				DespawnThisEnemy();
				return;
			}

			if (ce.Effect == CollisionEffect.Knockback)
			{
				ApplyKnockback(
					ce.Direction,
					ce.EffectIntensity,
					ce.EffectDuration);
			}
			else if (ce.Effect == CollisionEffect.Stopping)
			{
				ApplyStopping(ce.EffectDuration);
			}
			else if (ce.Effect == CollisionEffect.Block)
			{
				// player와 충돌해서 이벤트를 전송하고 스스로 despawn한다
				if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(ce.SenderId, out var obj))
				{
					_collisionEventCache.Position = transform.position;
					var pp = obj.GetComponent<INetworkObjectCollision>();
					pp.SendCollisionEvent(
						new CollisionEvent().FromCollisionEventStruct(_collisionEventCache));
				}

				DespawnThisEnemy();
				return;
			}
		}

		if (!IsEffectInProgress)
		{
			if (_path.corners.Count() > 0)
			{
				if ((transform.position - _path.corners[1]).magnitude < 0.25f
					&& _path.corners.Count() > 2)
				{
					MoveToTarget(_path.corners[2]);
				}
				else
				{
					MoveToTarget(_path.corners[1]);
				}
			}
		}
	}

	IEnumerator CalculatePathToTarget()
	{
		yield return null;

		var delay = new WaitForSeconds(0.2f);
		while (true)
		{
			if (Target != null)
			{
				bool res = NavMesh.CalculatePath(
					transform.position,
					Target.transform.position,
					NavMesh.AllAreas,
					_path);
				if (res)
				{
#if UNITY_EDITOR
					//Vector3 prePoint = transform.position;
					//foreach (var point in _path.corners)
					//{
					//	Debug.DrawLine(prePoint, point, Color.cyan);
					//	prePoint = point;
					//}
#endif
				}
			}

			yield return delay;
		}
	}

	void DespawnThisEnemy()
	{
		IPDS.CreateEffect(
			"EffectPop",
			transform.position,
			Quaternion.identity,
			new EffectRpcParameter()
			{
				EffectColor = new Color(1f, 0f, 0f)
			});
		DespawnEnemyRpc();
	}

	[Rpc(SendTo.Server)]
	public void AddCollisionEventRpc(CollisionEventStruct ce)
	{
		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ce));
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
