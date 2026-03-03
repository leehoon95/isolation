using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Events;

/*
 * 장탄수
 */
public class WeaponBolt : MonoBehaviour, IWeaponInterface
{
	[SerializeField]
	GameObject _muzzlePositionObject;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	int _round;
	[SerializeField]
	float _firingInterval;
	[SerializeField]
	float _bulletSpeed;
	[SerializeField]
	float _lifeTime;
	[SerializeField]
	float _minimumDistance;
	[SerializeField]
	CollisionEffect _collisionEffect;
	long _lastFiredTime;
	bool _triggerd;

	public Transform Transform { get => transform; }
	public Transform MuzzleTransform { get => _muzzlePositionObject.transform; }
	public PooledDynamicSpawner PDS { get; set; }
	public Color PersonalColor { get; set; }
	public float ChargingTime { get; set; }
	public int Round { get => _round; set => _round = value; }

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
			//	PDS.CreateProjectile(
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
			_triggerd = false;
		}
	}

	public bool Trigger(bool on)
	{
		_triggerd = on;
		return true;
	}

	public bool SetEvent(string eventName, float time)
	{
		GLogger.Log($"Weapon event: {eventName} {time}");
		return true;
	}
}
