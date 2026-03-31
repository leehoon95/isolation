using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;


public class BulletBolt : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
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
	bool _hit;
	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Knockback,
		EffectDuration = 0f,
		EffectIntensity = 0f,
		Damage = 10
	};

	public Color EffectColor
	{
		get => _effectColor;
		private set
		{
			_effectColor = value;
		}
	}

	void OnEnable()
	{
		StopAllCoroutines();
		_hit = false;
		_actualSpeed = _speed;
		_actualSpeedDeltaPerSec = _speedDeltaPerSec;
		_actualMaxAngularVelocity = _maxAngularVelocity;
		StartCoroutine(IgnoreStaticObjectForMoment(0.35f));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
	}

	void FixedUpdate()
	{
		if (!IsIllusion && _hit)
		{
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

		if (ci != null && !_hit)
		{
			_collisionEvent.Position = transform.position;
			_collisionEvent.Direction = transform.right;
			ci.SendCollisionEvent(_collisionEvent);
			_hit = true;
		}
	}

	IEnumerator IgnoreStaticObjectForMoment(float time)
	{
		_collider.excludeLayers = 1 << LayerMask.NameToLayer("Static Object");
		yield return new WaitForSeconds(time);
		_collider.excludeLayers = 0;
	}

	/*
	 * Spawner에서 IsIllusion을 먼저 설정하고 호출할 것
	 */
	public void SetProjectileParameter(in ProjectileRpcParameter param)
	{
		_startPosition = param.StartPosition;
		_targetPosition = param.TartgetPosition;
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
			//_collider.includeLayers = (LayerMask)param.CollisionIncludeLayers;
			//_collider.excludeLayers = (LayerMask)param.CollisionExcludeLayers;
			SetLifeTime(param.LifeTime);
		}
	}
}
