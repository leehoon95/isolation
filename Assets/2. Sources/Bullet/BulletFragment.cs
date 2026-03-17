using System.Collections.Generic;
using UnityEngine;

public class BulletFragment : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Collider2D _collider;

	int _contactedCount;
	int _eventSendCount;
	int _enemyTriggerLayer;

	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Stopping,
		EffectDuration = 0f,
		EffectIntensity = 0f,
		Damage = 26
	};

	void Start()
	{
		_enemyTriggerLayer = LayerMask.NameToLayer("Enemy Trigger");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	void FixedUpdate()
	{
		if (IsIllusion)
		{
			return;
		}

		var filter = new ContactFilter2D();
		filter.useTriggers = true;
		filter.SetLayerMask(1 << _enemyTriggerLayer);

		List<Collider2D> results = new();
		int count = _collider.Overlap(filter, results);

		if (count > 0)
		{
			//GLogger.Log($"Fragment Hit {count} {results.Count}");
			foreach (Collider2D collider in results)
			{
				var ci = collider.GetComponentInParent<INetworkObjectCollision>();


				if (ci != null)
				{
					ci.SendCollisionEvent(_collisionEvent);
					IPDS.CreateEffect(
						"EffectDamage",
						transform.position,
						Quaternion.identity,
						new EffectRpcParameter()
						{
							EffectColor = Color.white,
							Data1 = _collisionEvent.Damage
						});
				}
			}

			ReleaseObject();
		}
	}

	public void SetProjectileParameter(in ProjectileRpcParameter param)
	{
		if (IsIllusion)
		{
			_collider.enabled = false;
		}
		else
		{
			_collider.enabled = true;
			_collisionEvent.Effect = param.CollisionEvent.Effect;
			_collisionEvent.EffectIntensity = param.CollisionEvent.EffectIntensity;
			_collisionEvent.EffectDuration = param.CollisionEvent.EffectDuration;
			_collisionEvent.Damage = param.CollisionEvent.Damage;
			SetLifeTime(param.LifeTime);
		}
	}
}
