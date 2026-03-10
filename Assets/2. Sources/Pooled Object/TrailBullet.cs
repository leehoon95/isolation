using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;


public class TrailBullet : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	TrailRenderer _trailRenderer;

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
			_trailRenderer.colorGradient = new Gradient()
			{
				mode = GradientMode.Blend,
				colorKeys = new GradientColorKey[2]
				{
					new GradientColorKey(_effectColor, 0f),
					new GradientColorKey(_effectColor, 1f)
				}
			};
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_trailRenderer.Clear();
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
		
		if (_flyingType == ProjectileFlyingType.Rectilinear)
		{
			var direction = (_targetPosition - _startPosition).normalized;
			_rigidbody.MovePosition((Vector2)transform.position + direction * _speed * Time.fixedDeltaTime);
			_speed += _speedDeltaPerSec * Time.fixedDeltaTime;
		}
		else if (_flyingType == ProjectileFlyingType.Homing)
		{
			var distance = (_targetPosition - (Vector2)transform.position).magnitude;
			var direction = (_targetPosition - (Vector2)transform.position).normalized;
			var angle = Vector2.Angle(transform.right, direction);
			
			if (angle < 100f && distance > 0.5f/* && Mathf.Abs(angle) > 0.2f*/)
			{
				var cross = Vector3.Cross(transform.right, (Vector3)direction);
				var isTargetRightSide = cross.z > 0f;
				var maxHomingAngle = _maxAngulaVelocity * Time.fixedDeltaTime;

				var homingAngle = (angle > maxHomingAngle ? maxHomingAngle : angle) * (isTargetRightSide ? 1f : -1f);
				transform.Rotate(0, 0, homingAngle);
			}
			
			_rigidbody.MovePosition((Vector2)transform.position + (Vector2)transform.right * _speed * Time.fixedDeltaTime);
			_speed += _speedDeltaPerSec * Time.fixedDeltaTime;
		}
	}
	//void OnDrawGizmos()
	//{
	//	Gizmos.color = Color.blue;
	//	Gizmos.DrawLine(transform.position, transform.position + transform.rotation * Vector2.right);
	//}

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

	/*
	 * Spawner에서 IsIllusion을 먼저 설정하고 호출할 것
	 */
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
			//_collider.includeLayers = (LayerMask)param.CollisionIncludeLayers;
			//_collider.excludeLayers = (LayerMask)param.CollisionExcludeLayers;
			SetLifeTime(param.LifeTime);
		}
	}
}
