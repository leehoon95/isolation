using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackerEnemy : EnemyBase, INetworkObjectCollision
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	Collider2D _colliderTrigger;
	[SerializeField]
	string _attackProjectileName;
	[SerializeField]
	List<GameObject> _teeth;

	List<CollisionEvent> _collisionEventList;
	CollisionEventStruct _collisionEventCache;
	NavMeshPath _path;
	Coroutine _calculatePathCo;

	List<float> _teethSpeed = new();
	long _lastFiredTime;
	ProjectileRpcParameter _attackProjectileCache;


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

		base.OnNetworkSpawn();
		HealthPoint = MaxHealthPoint;
		_collisionEventList = new();
		// player 충돌
		_collisionEventCache = new()
		{
			SenderId = 0,
			Damage = 10,
			Effect = CollisionEffect.Hit,
		};
		_attackProjectileCache = new()
		{
			CollisionEvent = new CollisionEventStruct()
			{
				SenderId = 0,
				Effect = CollisionEffect.Hit,
				Damage = 5
			},
			LifeTime = 8f,
			EffectColor = new Color(0f, 1f, 170f / 255f)
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
			//GLogger.Log($"damage: {HealthPoint} {ce.Damage}");
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
				if (NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(ce.SenderId, out var obj))
				{
					_collisionEventCache.Position = transform.position;
					var pp = obj.GetComponent<PointmanPlayer>();
					pp.SendCollisionEvent(
						new CollisionEvent().FromCollisionEventStruct(_collisionEventCache));
				}

				DespawnThisEnemy();
				return;
			}
		}

		if (!IsEffectInProgress)
		{
			if (Target != null)
			{
				var targetDirection = Target.position - transform.position;
				var distance = targetDirection.magnitude;
				long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
				if ((now - _lastFiredTime) >= 3000 && distance < 12f)
				{
					_attackProjectileCache.StartPosition = transform.position;
					_attackProjectileCache.TartgetPosition = Target.position;

					IPDS.CreateProjectile(
						_attackProjectileName,
						transform.position,
						//Quaternion.LookRotation(targetDirection, Vector3.back),
						//Quaternion.AngleAxis(Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg, Vector3.forward),
						Quaternion.Euler(0f, 0f, Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg),
						_attackProjectileCache);

					_lastFiredTime = now;
				}

				
			}

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
					//GLogger.Log($"path status {_path.status} path count: {_path.corners.Count()}");
#if UNITY_EDITOR
					Vector3 prePoint = transform.position;
					foreach (var point in _path.corners)
					{
						Debug.DrawLine(prePoint, point, Color.cyan);
						prePoint = point;
					}
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
				EffectColor = new Color(0f, 1f, 170f / 255f)
			});
		DespawnEnemyRpc();
	}
	public void SendCollisionEvent(CollisionEvent ce)
	{
		AddCollisionEventRpc(ce);
	}

	[Rpc(SendTo.Server)]
	public void AddCollisionEventRpc(CollisionEventStruct ce)
	{
		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ce));
	}

	public CollisionEvent GetCollisionEvent()
	{
		return new CollisionEvent().FromCollisionEventStruct(_collisionEventCache);
	}
}
