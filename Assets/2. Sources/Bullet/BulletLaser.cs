using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class BulletLaser : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Collider2D _collider;

	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Hit,
		Damage = 2
	};
	ContactFilter2D _contactFilter = new()
	{
		useTriggers = true,
		useLayerMask = true,
	};
	List<Collider2D> _results = new();

	void Start()
	{
		_contactFilter.layerMask = 1 << LayerMask.NameToLayer("Enemy Trigger");
	}

	void FixedUpdate()
	{
		if (IsIllusion)
		{
			return;
		}

		int count = _collider.Overlap(_contactFilter, _results);

		if (count > 0)
		{
			_collisionEvent.Position = transform.position;
			foreach (Collider2D collider in _results)
			{
				var ci = collider.GetComponentInParent<INetworkObjectCollision>();
				var ce = ci.GetCollisionEvent();

				if (ci != null)
				{
					ci.SendCollisionEvent(_collisionEvent);

					if (ce.Effect != CollisionEffect.None)
					{
						var closestPoint = collider.ClosestPoint(transform.position);
						var erp = new EffectRpcParameter()
						{
							EffectColor = Color.white
						};
						erp.Data.Append(_collisionEvent.Damage);

						IPDS.CreateEffect(
							"EffectDamage",
							closestPoint,
							Quaternion.identity,
							erp);

						break;
					}
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
			_collisionEvent.Damage = param.CollisionEvent.Damage;
			SetLifeTime(param.LifeTime);
		}
	}
}
