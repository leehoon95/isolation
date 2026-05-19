using UnityEngine;

public class WeaponBolt : MonoBehaviour, IWeaponInterface
{
	[SerializeField]
	string _projectileName;
	[SerializeField]
	long _firingInterval;
	[SerializeField]
	float _knockbackDuration;
	[SerializeField]
	float _knockbackIntensity;
	[SerializeField]
	int _damage;

	long _lastFiredMilliSecTick;
	string _buff = "";
	AudioContainer _ac;

	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName => "bolt";
	public GameObject GO => gameObject;

	void Start()
	{
		_ac = AudioContainer.Instance;
	}

	void OnDestroy()
	{
		StopAllCoroutines();
	}

	public void Trigger(bool on)
	{
		if (!on)
		{
			return;
		}

		long firingInterval;
		bool burst = _buff == "burst";

		if (burst)
		{
			firingInterval = (long)(_firingInterval * 0.8f);
		}
		else
		{
			firingInterval = _firingInterval;
		}

		var now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
		var toTargetVector = TargetPosition - (Vector2)Muzzle.position;
		var toTargetCenterVector = TargetPosition - (Vector2)Muzzle.parent.position;
		
		if (toTargetCenterVector.magnitude < 1f)
		{
			return;
		}

		var distanceFromTarget = toTargetVector.magnitude;
		var correctedAccuracy = Mathf.Lerp(0f, 0.25f, distanceFromTarget / 6f);

		if (now - _lastFiredMilliSecTick >= _firingInterval)
		{
			var angle = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
			IPDS.CreateProjectile(
				_projectileName,
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
						EffectDuration = _knockbackDuration,
						EffectIntensity = _knockbackIntensity,
						Damage = _damage
					},
					EffectColor = PersonalColor,
					LifeTime = 5f,
				});
			_ac.PlayAudio("toygun", Muzzle.position);

			_lastFiredMilliSecTick = now;
		}
	}

	public void ApplyBuff(string buffName)
	{
		_buff = buffName;
	}

	public void RemoveBuff(string buffName)
	{
		_buff = "";
	}

	public void Stop()
	{
		StopAllCoroutines();
	}
}
