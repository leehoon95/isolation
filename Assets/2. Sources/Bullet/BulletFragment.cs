using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletFragment : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Collider2D _collider;

	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Stopping,
		EffectDuration = 0f,
		EffectIntensity = 0f,
		Damage = 12
	};
	ContactFilter2D _contactFilter = new()
	{
		useTriggers = true,
		useLayerMask = true,
	};
	List<Collider2D> _results = new();
	Coroutine _ReduceSizeCo;

	void Start()
	{
		_contactFilter.layerMask = 1 << LayerMask.NameToLayer("Enemy Trigger");
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

		int count = _collider.Overlap(_contactFilter, _results);

		if (count > 0)
		{
			_collisionEvent.Position = transform.position;
			foreach (Collider2D collider in _results)
			{
				var ci = collider.GetComponentInParent<INetworkObjectCollision>();

				if (ci != null)
				{
					ci.SendCollisionEvent(_collisionEvent);
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
