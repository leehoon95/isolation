using UnityEngine;

public class WeaponLaser : IWeaponInterface
{
	long _lastFiredMilliSecTick;

	public string ProjectileName { get; set; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public long FiringInterval { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName => "laser";

	public void Trigger(bool on)
	{
		if (!on)
		{
			return;
		}

		long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

		var toTargetVector = TargetPosition - (Vector2)Muzzle.position;
		var toTargetCenterVector = TargetPosition - (Vector2)Muzzle.parent.position;

		if (toTargetCenterVector.magnitude < 1f)
		{
			return;
		}

		var distanceFromTarget = toTargetVector.magnitude;
		var correctedAccuracy = Mathf.Lerp(0f, 0.25f, distanceFromTarget / 6f);

		if (now - _lastFiredMilliSecTick >= FiringInterval)
		{
			var angle = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
			IPDS.CreateProjectile(
				ProjectileName,
				(Vector2)Muzzle.position,
				Quaternion.Euler(0f, 0f, angle),
				new ProjectileRpcParameter()
				{
					StartPosition = (Vector2)Muzzle.position,
					TartgetPosition = TargetPosition + Random.insideUnitCircle * correctedAccuracy,
					CollisionEvent = new CollisionEventStruct()
					{
						SenderId = ClientId,
						Effect = CollisionEffect.Knockback,
						EffectDuration = 0.025f,
						EffectIntensity = 1.5f,
						Damage = 12
					},
					EffectColor = PersonalColor,
					LifeTime = 5f,
				});
			_lastFiredMilliSecTick = now;
		}
	}

	public void AddBuff(string buffName, float time)
	{
		
	}


}
