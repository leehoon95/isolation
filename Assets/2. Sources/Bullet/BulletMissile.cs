using System.Collections;
using UnityEngine;

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
	string _fragmentProjectileName;
	[SerializeField]
	int _fragmentDamage;
	[SerializeField]
	float _speed;
	[SerializeField]
	float _speedDeltaPerSec;
	[SerializeField]
	float _maxAngularVelocity;
	[SerializeField]
	float _stoppingDuration;

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
		_hit = false;
		_actualSpeed = _speed;
		_actualSpeedDeltaPerSec = _speedDeltaPerSec;
		_actualMaxAngularVelocity = _maxAngularVelocity;
		StartCoroutine(IgnoreStaticObjectForMoment(0.35f));
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_trailRenderer.Clear();
	}

	void FixedUpdate()
	{
		if (!IsIllusion && _hit)
		{
			IPDS.CreateEffect(
				"EffectNoise",
				transform.position,
				Quaternion.identity,
				new EffectRpcParameter()
				{
					EffectColor = EffectColor
				});

			IPDS.CreateProjectile(
				_fragmentProjectileName,
				(Vector2)transform.position,
				Quaternion.identity,
				new ProjectileRpcParameter()
				{
					CollisionEvent = new CollisionEventStruct()
					{
						SenderId = OwnerClientId,
						Effect = CollisionEffect.Stopping,
						EffectDuration =_stoppingDuration,
						EffectIntensity = 0f,
						Damage = _fragmentDamage
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
