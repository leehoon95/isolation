using UnityEngine;


public class NormalBullet : PooledProjectileBase, IColliderInteractable
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	TrailRenderer _trailRenderer;

	LayerMask _collisionMask;

	public float Speed { get; private set; }
	public Vector2 Direction { get; private set; }
	public LayerMask CollisionMask 
	{ 
		get => _collisionMask;
		set 
		{
			_collisionMask = value;
			_collider.includeLayers = _collisionMask;
		}
	}
	public CollisionEffect CollisionEffect { get; private set; }
	public string CollisionEffectDetail { get; private set; }
	public Color BulletColor { get; private set; }

	protected override void OnDisable()
	{
		base.OnDisable();
		_trailRenderer.Clear();
	}

	void FixedUpdate()
	{
		_rigidbody.linearVelocity = transform.up * Speed;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsIllusion)
		{
			return;
		}

		if (((1 << collision.gameObject.layer) & CollisionMask.value) != 0)
		{
			var ci = collision.gameObject.GetComponent<IColliderInteractable>();
			if (ci != null)
			{
				ci.AddCollisionEvent(new CollisionEvent()
				{
					From = gameObject.transform.position,
					To = Direction,
					Effect = CollisionEffect.Damage,
				});
				var effect = ci.GetEffect();
				
			}
			
		}
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
		if (IsIllusion)
		{
			return;
		}

	}

	public CollisionEffect GetEffect()
	{
		return CollisionEffect.Damage;
	}

	public void SetProjectileParameter(in ProjectileRpcParameter param)
	{
		Speed = param.Speed;
		BulletColor = param.ProjectileColor;

		if (IsIllusion)
		{
			CollisionMask = param.CollisionMask;
			CollisionEffect = (CollisionEffect)param.CollisionEffect;
			CollisionEffectDetail = param.CollisionEffectDetail.ToString();
			SetLifeTime(param.LifeTime);
		}
		else
		{
			CollisionMask = (LayerMask)0;
			CollisionEffect = CollisionEffect.None;
			CollisionEffectDetail = "";
		}
	}
}
