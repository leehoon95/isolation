using UnityEngine;

public class WeaponMissile : MonoBehaviour, IWeaponInterface
{
	[SerializeField]
	string _projectileName;
	[SerializeField]
	long _firingInterval;
	[SerializeField]
	int _damage;


	long _lastFiredMilliSecTick;
	string _buff;

	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName => "missile";
	public GameObject GO => gameObject;

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

		long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;

		var toTargetVector = TargetPosition - (Vector2)Muzzle.position;
		var toTargetCenterVector = TargetPosition - (Vector2)Muzzle.parent.position;
		
		if (toTargetCenterVector.magnitude < 2f)
		{
			return;
		}

		var distanceFromTarget = toTargetVector.magnitude;
		var correctedAccuracy = Mathf.Lerp(0f, 0.5f, distanceFromTarget / 5f);

		if (now - _lastFiredMilliSecTick >= _firingInterval)
		{
			var angle = Mathf.Atan2(toTargetVector.y, toTargetVector.x) * Mathf.Rad2Deg;
			var rotation = Quaternion.Euler(0f, 0f, angle + (IsRightWeapon ? -90f : 90f));
			var prp = new ProjectileRpcParameter()
				{
					StartPosition = (Vector2)Muzzle.position,
					TartgetPosition = TargetPosition + Random.insideUnitCircle * correctedAccuracy,
					CollisionEvent = new CollisionEventStruct()
					{
						SenderId = ClientId,
						Effect = CollisionEffect.Hit,
						Damage = _damage
					},
					EffectColor = PersonalColor,
					LifeTime = 5f,
				};

			IPDS.CreateProjectile(
				_projectileName,
				(Vector2)Muzzle.position,
				rotation,
				prp
				);

			if (_buff == "burst")
			{
				var leftCrossVec = (Vector2)Vector3.Cross(toTargetVector, Vector3.forward).normalized;
				var rightCrossVec = (Vector2)Vector3.Cross(toTargetVector, Vector3.back).normalized;
				var targetPosition = prp.TartgetPosition;
				var leftTartgetPosition = targetPosition + leftCrossVec + Random.insideUnitCircle * correctedAccuracy;
				var rightTartgetPosition = targetPosition + rightCrossVec + Random.insideUnitCircle * correctedAccuracy;

				prp.TartgetPosition = leftTartgetPosition;
				IPDS.CreateProjectile(
					_projectileName,
					(Vector2)Muzzle.position,
					rotation,
					prp);

				prp.TartgetPosition = rightTartgetPosition;
				IPDS.CreateProjectile(
					_projectileName,
					(Vector2)Muzzle.position,
					rotation,
					prp);
			}

			_lastFiredMilliSecTick = now;
		}
	}

	public void AddBuff(string buffName, float time)
	{
		GLogger.Log($"Weapon missile buff added: {buffName} {time}");
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
