using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using TMPro;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Splines;


public class BulletMissile : PooledProjectileBase, IProjectileSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	TrailRenderer _trailRenderer;
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
	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEvent = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Knockback,
		EffectDuration = 0f,
		EffectIntensity = 0f,
	};

	public Color EffectColor
	{
		get => _effectColor;
		private set
		{
			_effectColor = value;
			_spriteRenderer.color = _effectColor;
			_trailRenderer.colorGradient = new Gradient()
			{
				mode = GradientMode.Blend,
				colorKeys = new GradientColorKey[2]
				{
					new GradientColorKey(_effectColor, 0f),
					new GradientColorKey(new Color(0f, 0f, 0f, 0f), 1f)
				}
			};
		}
	}

	void OnEnable()
	{
		StopAllCoroutines();
		_actualSpeed = _speed;
		_actualSpeedDeltaPerSec = _speedDeltaPerSec;
		_actualMaxAngularVelocity = _maxAngularVelocity;
		StartCoroutine(IgnoreStaticObjectForMoment(0.45f));
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
			_collisionEventList.Clear();
			IPDS.CreateEffect(
				"EffectDamage",
				transform.position,
				Quaternion.identity,
				new EffectRpcParameter()
				{
					EffectColor = Color.white,
					Data1 = _collisionEvent.Damage
				});

			IPDS.CreateEffect(
				"EffectNoise",
				transform.position,
				Quaternion.identity,
				new EffectRpcParameter()
				{
					EffectColor = EffectColor
				});

			IPDS.CreateProjectile(
				"FragmentBullet",
				(Vector2)transform.position,
				Quaternion.identity,
				new ProjectileRpcParameter()
				{
					CollisionEvent = new CollisionEventStruct()
					{
						SenderId = OwnerClientId,
						Effect = CollisionEffect.Stopping,
						EffectDuration = 0.333f,
						EffectIntensity = 0f,
						Damage = 16
					},
					LifeTime = 1f
				});

			ReleaseObject();
			return;
		}

		var distance = (_targetPosition - (Vector2)transform.position).magnitude;
		var direction = (_targetPosition - (Vector2)transform.position).normalized;
		var angle = Vector2.Angle(transform.right, direction);
			
		if (angle < 120f && distance > 0.5f/* && Mathf.Abs(angle) > 0.2f*/)
		{
			var cross = Vector3.Cross(transform.right, (Vector3)direction);
			var isTargetRightSide = cross.z > 0f;
			var maxHomingAngle = _actualMaxAngularVelocity * Time.fixedDeltaTime;

			var homingAngle = (angle > maxHomingAngle ? maxHomingAngle : angle) * (isTargetRightSide ? 1f : -1f);
			transform.Rotate(0, 0, homingAngle);
		}
			
		_rigidbody.MovePosition((Vector2)transform.position + (Vector2)transform.right * _actualSpeed * Time.fixedDeltaTime);
		_actualSpeed += _actualSpeedDeltaPerSec * Time.fixedDeltaTime;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (IsIllusion)
		{
			return;
		}

		var ci = collision.GetComponentInParent<INetworkObjectCollision>();

		if (ci != null && _collisionEventList.Count == 0)
		{
			_collisionEvent.Position = transform.position;
			_collisionEvent.Direction = transform.right;
			ci.SendCollisionEvent(_collisionEvent);
			var ce = ci.GetCollisionEvent();
			_collisionEventList.Add(ce);
			if (ce.Effect != CollisionEffect.None)
			{
				IPDS.CreateEffect(
					"EffectDamage",
					transform.position,
					Quaternion.identity,
					new EffectRpcParameter()
					{
						EffectColor = EffectColor,
						Data1 = _collisionEvent.Damage
					});
			}
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
