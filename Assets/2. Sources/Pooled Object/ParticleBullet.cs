using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine;

public class ParticleBullet : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	ParticleSystem _particlSystem;

	ProjectileFlyingType _flyingType;
	Vector2 _startPosition;
	Vector2 _targetPosition;
	float _speed;
	float _speedDeltaPerSec;
	float _maxAngulaVelocity;
	Color _effectColor;
	float _lifeTime;
	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Knockback,
		EffectDuration = 0f,
		EffectIntensity = 0f,
		Damage = 20
	};

	public Color EffectColor
	{
		get => _effectColor;
		private set
		{
			_effectColor = value;
			_spriteRenderer.color = value;
			var main = _particlSystem.main; 
			main.startColor = value;
		}
	}

	void FixedUpdate()
	{
		if (!IsIllusion && _collisionEventList.Count > 0)
		{
			//var ce = _collisionEventList.First();
			//_collisionEventList.RemoveAt(0);
			_collisionEventList.Clear();
			ReleaseObject();
			return;
		}

		var direction = (_targetPosition - _startPosition).normalized;
		_rigidbody.MovePosition((Vector2)transform.position + direction * _speed * Time.fixedDeltaTime);
		_speed += _speedDeltaPerSec * Time.fixedDeltaTime;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsIllusion)
		{
			return;
		}

		var ci = collision.GetComponentInParent<INetworkObjectCollision>();

		if (ci != null)
		{
			_collisionEvent.Position = transform.position;
			_collisionEvent.Direction = transform.right;
			ci.SendCollisionEvent(_collisionEvent);
			var ce = ci.GetCollisionEvent();
			_collisionEventList.Add(ce);
		}
	}

	public void SetProjectileParameter(in ProjectileRpcParameter param)
	{
		_flyingType = param.FlyingType;
		_startPosition = param.StartPosition;
		_targetPosition = param.TartgetPosition;
		_speed = param.Speed;
		_speedDeltaPerSec = param.SpeedDeltaPerSec;
		_maxAngulaVelocity = param.MaxAngularVelocity;
		EffectColor = param.EffectColor;

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
