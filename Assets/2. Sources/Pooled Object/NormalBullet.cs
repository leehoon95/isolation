using JetBrains.Annotations;
using System.Data;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;


public class NormalBullet : PooledProjectileBase, IProjectileSetting, ICollisionInteractable
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	TrailRenderer _trailRenderer;

	ProjectileFlyingType _flyingType;
	Vector2 _startPosition;
	Vector2 _targetPosition;
	float _speed;
	float _speedDeltaPerSec;
	float _maxAngulaVelocity;
	CollisionEffect _collisionEffect;
	string _collisionEffectDetail;
	Color _effectColor;
	float _lifeTime;

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
					new GradientColorKey(Color.white, 0f),
					new GradientColorKey(Color.yellow, 1f)
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
		//GLogger.Log($"illusion {IsIllusion}");
		if (IsIllusion)
		{
			return;
		}

		var ci = collision.GetComponentInParent<ICollisionInteractable>();
		if (ci == null)
		{
			ci = collision.GetComponent<ICollisionInteractable>();
		}

		if (ci != null)
		{
			ci.AddCollisionEvent(new CollisionEvent()
			{
				Position = gameObject.transform.position,
				Direction = transform.rotation * Vector2.right,
				Effect = CollisionEffect.Damage,
				EffectDetail = _collisionEffectDetail,
			});
			var effect = ci.GetEffect();

			if (effect == CollisionEffect.None)
			{
				ReleaseObject();
			}
		}
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
	}

	public CollisionEffect GetEffect()
	{
		return CollisionEffect.Damage;
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
		_collisionEffect = (CollisionEffect)param.CollisionEffect;
		_collisionEffectDetail = param.CollisionEffectDetail.ToString();
		EffectColor = param.EffectColor;

		//var direction = _targetPosition - _startPosition;
		//transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

		if (IsIllusion)
		{
			_collider.enabled = false;
		}
		else
		{
			_collider.enabled = true;
			//_collider.includeLayers = (LayerMask)param.CollisionIncludeLayers;
			//_collider.excludeLayers = (LayerMask)param.CollisionExcludeLayers;
			SetLifeTime(param.LifeTime);
		}
	}

	//float FourPointBezier(float a, float b, float c, float d, float t)
	//{
	//	return Mathf.Pow(1 - t, 3) * a +
	//		Mathf.Pow(1  - t, 2) * 3 * t * b +
	//		Mathf.Pow(t, 2) * 3 * (1 - t) * c +
	//		Mathf.Pow(t, 3) * d;
	//}

	//float FivePointBezier(float p0, float p1, float p2, float p3, float p4, float t)
	//{
	//	float u = 1 - t;
	//	float tt = t * t;
	//	float uu = u * u;
	//	float ttt = tt * t;
	//	float uuu = uu * u;
	//	float tttt = ttt * t;
	//	float uuuu = uuu * u;

	//	return (uuuu * p0) +
	//				   (4 * uuu * t * p1) +
	//				   (6 * uu * tt * p2) +
	//				   (4 * u * ttt * p3) +
	//				   (tttt * p4);
	//}

	//public Vector2 CalculateBezierPoints3(float t, Vector2[] points)
	//{
	//	// 안전 장치: 3차 곡선은 4개의 점이 필요합니다.
	//	if (points == null || points.Length < 4)
	//	{
	//		Debug.LogError("3차 베지에 곡선을 위해서는 최소 4개의 제어점이 필요합니다.");
	//		return Vector2.zero;
	//	}

	//	t = Mathf.Clamp01(t);
	//	float u = 1f - t; // (1-t)

	//	// 계수 계산 (Optimization: Pow 대신 직접 곱셈)
	//	float tt = t * t;     // t^2
	//	float ttt = tt * t;   // t^3
	//	float uu = u * u;     // (1-t)^2
	//	float uuu = uu * u;   // (1-t)^3

	//	// B(t) = (1-t)^3*P0 + 3(1-t)^2*t*P1 + 3(1-t)*t^2*P2 + t^3*P3
	//	Vector2 result =
	//		uuu * points[0] +
	//		3f * uu * t * points[1] +
	//		3f * u * tt * points[2] +
	//		ttt * points[3];

	//	return result;
	//}

	//Vector2 CalculateBezierPoints4(float t, Vector2[] points)
	//{
	//	// 안전 장치: 4차 곡선은 5개의 점이 필요합니다.
	//	if (points == null || points.Length < 5)
	//	{
	//		Debug.LogError("4차 베지에 곡선을 위해서는 최소 5개의 제어점이 필요합니다.");
	//		return Vector2.zero;
	//	}

	//	t = Mathf.Clamp01(t);
	//	float oneMinusT = 1f - t;

	//	// 계수 계산 (Optimization: Pow 대신 곱셈 사용)
	//	float t2 = t * t;
	//	float t3 = t2 * t;
	//	float t4 = t3 * t;

	//	float u = oneMinusT;
	//	float u2 = u * u;
	//	float u3 = u2 * u;
	//	float u4 = u3 * u;

	//	// B(t) = (1-t)^4*P0 + 4(1-t)^3*t*P1 + 6(1-t)^2*t^2*P2 + 4(1-t)*t^3*P3 + t^4*P4
	//	Vector2 result =
	//		u4 * points[0] +
	//		4f * u3 * t * points[1] +
	//		6f * u2 * t2 * points[2] +
	//		4f * u * t3 * points[3] +
	//		t4 * points[4];

	//	return result;
	//}
}
