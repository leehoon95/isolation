using System;
using System.Collections.Generic;
using UnityEngine;



// deprecated
public class Weapon : MonoBehaviour
{
	[SerializeField]
	GameObject _muzzlePositionObject;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	CollisionEffect _collisionEffect;
	[SerializeField]
	List<WeaponConfig> _weaponConfig;

	bool _triggerd;
	bool _isRightWeapon;
	Vector2 _targetPosition;
	Dictionary<string, WeaponConfig> _configs;
	string _currentWeapon;

	// IWeaponInterface
	public GameObject Projectile { get; set; }
	public Transform MuzzleTransform { get => _muzzlePositionObject.transform; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon
	{
		get => _isRightWeapon;
		set
		{
			_isRightWeapon = value;
			_spriteRenderer.flipX = value;
		}
	}
	public Vector2 TargetPosition
	{
		get => _targetPosition;
		set => _targetPosition = value;
	}
	public string CurrentWeapon
	{
		get => _currentWeapon;
		set
		{
			_currentWeapon = value;

		}
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	void FixedUpdate()
	{
		if (_triggerd)
		{
			//long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			//if ((now - _lastFiredTime) >= _firingInterval)
			//{
			//	IPDS.CreateProjectile(
			//		"BulletNormal",
			//		MuzzleTransform.position,
			//		MuzzleTransform.rotation,
			//		new ProjectileRpcParameter()
			//		{
			//			FlyingType = ProjectileFlyingType.Rectilinear,
			//			Speed = _bulletSpeed,
			//			CollisionEvent = new CollisionEventStruct()
			//			{
			//				Effect = CollisionEffect.Knockback,
			//				EffectDuration = 0.02f,
			//				EffectIntensity = 5f,
			//				Damage = 10,
			//			},
			//			EffectColor = PersonalColor,
			//			LifeTime = _lifeTime,
			//		});

			//	_lastFiredTime = now;
			//}
		}
	}

	public void Trigger(bool on)
	{
		_triggerd = on;
	}

	public bool SetEvent(string eventName, float time)
	{
		GLogger.Log($"Weapon event: {eventName} {time}");
		return true;
	}

	void SetWeapon(string weapon)
	{
		switch (weapon)
		{
			case "null":
				_spriteRenderer.sprite = null;
				break;

		}
	}

	public bool AddBuff(string buffName, float time)
	{
		throw new NotImplementedException();
	}
}
