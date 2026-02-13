using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public enum WeaponGripMode
{
	Left, Right, Both
}

public interface IPlayerSetting
{
	public string Nickname { set; }
	public Color PersonalColor { set; }
}


public class PointmanPlayer : PlayerBase, ICollisionInteractable, IPlayerSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	GameObject _leftWeaponPosition;
	[SerializeField]
	GameObject _rightWeaponPosition;
	[SerializeField]
	WeaponListSO _weaponList;
	[SerializeField]
	SpriteRenderer _bodySprite;
	[SerializeField]
	PlayerBodyIndicator _bodyIndicator;
	

	NetworkVariable<int> _health = new(
		100, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Owner);

	IWeaponInterface _leftWeapon;
	IWeaponInterface _rightWeapon;

	WeaponGripMode _weaponGripMode = WeaponGripMode.Right;
	List<CollisionEvent> _collisionEvents = new();
	List<string> _fieldEvent = new();

	string _nickname;
	Color _personalColor;
	float _angle;
	long _lastFiredTime = 0;

	public string Nickname
	{
		set
		{
			_nickname = value;
		}
	}

	public Color PersonalColor 
	{
		set
		{
			_personalColor = value;
			_bodyIndicator.PersonalColor = value;
		}
	}

	void Start()
	{
		
	}

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		if (!IsOwner)
		{
			return;
		}

		_health.OnValueChanged += HeathChanged;
		_health.Value = 100;
		//_fieldEvent.Add("Pistol");
	}

	//void OnTriggerEnter2D(Collider2D collision)
	//{
	//	var ci = collision.gameObject.GetComponentInParent<ICollisionInteractable>();
	//	if (ci != null)
	//	{

	//	}
	//}

	void Update()
	{
		if (IsOwner)
		{
			transform.rotation = Quaternion.Euler(0f, 0f, _angle);
		}
	}

	void FixedUpdate()
	{
		if (!IsOwner)
		{
			return;
		}

		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var toMouseVector = mousePosition - (Vector2)transform.position;
		float gunCorrectionAngle = 0f;

		/*
		 * 캐릭터 한 쪽에 정확히 조준해야 하는 로직이 있어야 하는 경우
		 */
		//if (_rightWeapon != null)
		//{
		//	var distance = (mousePosition - (Vector2)transform.position).magnitude;
		//	var gunWorldVector = _rightWeapon.MuzzleTransform.position - transform.position;
		//	/*
		//	 * 마우스가 캐릭터 중심과 gun muzzle 사이에 있으면 계산 불가
		//	 */
		//	if (distance > gunWorldVector.magnitude)
		//	{
		//		var gunX = Mathf.Abs(_rightWeaponPosition.transform.localPosition.x + _rightWeapon.MuzzleTransform.localPosition.x);
		//		var gunTargetY = Mathf.Sqrt(distance * distance - gunX * gunX);
		//		gunCorrectionAngle = Vector2.Angle(
		//			new Vector2(gunX, gunTargetY),
		//			new Vector2(0f, distance));
		//	}
		//	else
		//	{
		//		gunCorrectionAngle = 0f;
		//	}
		//}

		_angle = Mathf.Atan2(toMouseVector.y, toMouseVector.x) * Mathf.Rad2Deg + gunCorrectionAngle;

		if (MovementValue.magnitude > float.Epsilon)
		{
			
			var newPosition = _rigidbody.position + MovementValue.normalized * 5f * Time.fixedDeltaTime;
			_rigidbody.MovePosition(newPosition);
		}

		//if (_leftWeapon != null)
		{
			long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
			if (LeftTrigger && ((now - _lastFiredTime) >= 50))
			{
				var muzzle = mousePosition.normalized;
				PDS.CreateProjectile(
					"BulletNormal",
					(Vector2)transform.position,
					Quaternion.Euler(0f, 0f, _angle + 90),
					new ProjectileRpcParameter()
					{
						FlyingType = ProjectileFlyingType.Homing,
						StartPosition = transform.position,
						TartgetPosition = mousePosition + new Vector2(Random.Range(-0.2f, 0.2f), Random.Range(-0.5f, 0.5f)),
						Speed = 12f,
						SpeedDeltaPerSec = 0f,
						MaxAngularVelocity = 720f,
						CollisionEffect = (int)CollisionEffect.Damage,
						CollisionEffectDetail = "5",
						EffectColor = _personalColor,
						LifeTime = 5f,
					});

				_lastFiredTime = now;
			}
		}

		if (_fieldEvent.Count > 0)
		{
			foreach (var fieldEvent in _fieldEvent)
			{
				if (fieldEvent == "Pistol")
				{
					var instance = Instantiate(_weaponList.Pistol, _rightWeaponPosition.transform);
					instance.transform.localPosition = Vector2.zero;
					_rightWeapon = instance;
					_rightWeapon.PDS = PDS;
					_rightWeapon.PersonalColor = _personalColor;
				}

			}

			_fieldEvent.Clear();
		}
	}

	[Rpc(SendTo.Everyone)]
	void ProcessEventRpc()
	{

	}

	void ProcessEventImplementation()
	{

	}

	void HeathChanged(int previousValue, int newValue)
	{
		_bodyIndicator.Health = newValue;
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
		if (!IsOwner)
		{
			return;
		}

		_collisionEvents.Add(ce);
	}

	public CollisionEffect GetEffect()
	{
		return CollisionEffect.None;
	}
}
