using System.Collections.Generic;
using System.Net.WebSockets;
using UnityEngine;

public class BulletParticle : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	ParticleSystem _particlSystem;
	[Header("Spec")]
	[SerializeField]
	float _speed;
	[SerializeField]
	float _speedDeltaPerSec;
	[SerializeField]
	float _maxAngularVelocity;

	float _actualSpeed;
	float _actualSpeedDeltaPerSec;
	float _actualMaxAngularVelocity;
	Vector2 _startPosition;
	Vector2 _targetPosition;
	Color _effectColor;
	bool _hit = false;
	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Hit,
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

	void OnEnable()
	{
		_actualSpeed = _speed;
		_actualSpeedDeltaPerSec = _speedDeltaPerSec;
		_actualMaxAngularVelocity = _maxAngularVelocity;
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		_hit = false;
	}

	void FixedUpdate()
	{
		//if (!IsIllusion && _hit)
		if (_hit)
		{
			ReleaseObject();
			return;
		}

		var direction = (_targetPosition - _startPosition).normalized;
		_rigidbody.MovePosition((Vector2)transform.position + direction * _actualSpeed * Time.fixedDeltaTime);
		_actualSpeed += _actualSpeedDeltaPerSec * Time.fixedDeltaTime;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		//if (IsIllusion)
		//{
		//	return;
		//}

		var ci = collision.GetComponentInParent<INetworkObjectCollision>();

		if (ci != null && !_hit)
		{
			_collisionEvent.Position = transform.position;
			_collisionEvent.Direction = transform.right;
			ci.SendCollisionEvent(_collisionEvent);
			_hit = true;
		}
	}

	public void SetProjectileParameter(in ProjectileRpcParameter param)
	{
		_startPosition = param.StartPosition;
		_targetPosition = param.TartgetPosition;
		EffectColor = param.EffectColor;

		//if (IsIllusion)
		//{
		//	_collider.enabled = false;
		//}
		//else
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
