using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public struct LaserPointData : INetworkSerializable
{
	public ulong EnemyNetworkObjectId;
	public Vector2 LaserPointFromCenter;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref EnemyNetworkObjectId);
		serializer.SerializeValue(ref LaserPointFromCenter);
	}
}


public class WeaponLaser : MonoBehaviour, IWeaponInterface
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	string _projectileName;
	[SerializeField]
	int _totalFiringInterval;
	[SerializeField]
	int _hitCount;
	[SerializeField]
	int _damage;
	[SerializeField]
	List<EffectLaserHandler> _laserHandlers;

	int _maxLaserStreamCount;
	int _enemyTriggerLayer;
	List<Collider2D> _results = new();
	List<Vector2> _randomHitPoint = new();
	
	Coroutine _fireCo;
	Coroutine _laserCo;
	WaitForSeconds _remainderDelay;
	Transform[] _transformCache;
	string _buff = "";
	AudioContainer _ac;

	public string ProjectileName { get; set; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName => "laser";
	public GameObject GO => gameObject;

	public ILaserFiringRpc WCR { get; set; }

	void Start()
	{
		_ac = AudioContainer.Instance;
		_maxLaserStreamCount = _laserHandlers.Count;
		_enemyTriggerLayer = LayerMask.NameToLayer("Enemy Trigger");
		int remainder = _totalFiringInterval % _hitCount;
		if (remainder != 0)
		{
			_remainderDelay = new WaitForSeconds(remainder / 1000f);
		}

		for (int i = 0; i < _maxLaserStreamCount; ++i)
		{
			_laserHandlers[i].FiringDuration = _totalFiringInterval / 1000f;
			_laserHandlers[i].EffectColor = Color.green;
			_laserHandlers[i].transform.SetParent(null);
		}

		for (int i = 0; i < 8; ++i)
		{
			_randomHitPoint.Add(UnityEngine.Random.insideUnitCircle * 0.5f);
		}

		_transformCache = new Transform[_maxLaserStreamCount];
	}

	void OnDestroy()
	{
		StopAllCoroutines();
		for (int i = 0; i < _laserHandlers.Count; i++)
		{
			if (_laserHandlers[i] != null)
			{
				Destroy(_laserHandlers[i].gameObject);
			}
		}
		IPDS = null;
		WCR = null;
	}

	public void Trigger(bool on)
	{
		if (!on)
		{
			return;
		}

		if (_fireCo != null)
		{
			return;
		}

		_fireCo = StartCoroutine(FireLaser());
	}

	IEnumerator FireLaser()
	{
		var filter = new ContactFilter2D();
		filter.useTriggers = true;
		filter.SetLayerMask(1 << _enemyTriggerLayer);

		int collisionCount = _collider.Overlap(filter, _results);
		
		if (collisionCount == 0)
		{
			_fireCo = null;
			yield break;
		}

		bool burst = _buff == "burst";
		int hitCount = 0;

		if (burst)
		{
			hitCount = _hitCount * 2;
		}
		else
		{
			hitCount = _hitCount;
		}

		int streamCount = Mathf.Min(_maxLaserStreamCount, collisionCount);
		var lpd = new LaserPointData[streamCount];

		for (int i = 0; i < streamCount; i++)
		{
			var no = _results[i].GetComponentInParent<NetworkObject>();
			lpd[i].EnemyNetworkObjectId = no.NetworkObjectId;
			lpd[i].LaserPointFromCenter = _randomHitPoint[UnityEngine.Random.Range(0, 8)];
		}

		float hitInterval = (_totalFiringInterval / hitCount) / 1000f;

		WCR.FireLaserFromOtherClinent(IsRightWeapon, lpd, _buff);
		FireLaserStream(lpd, _buff);

		bool fire = false;
		float t = 0f;

		while (hitCount > 0)
		{
			if (t >= hitInterval)
			{
				fire = true;
				t -= hitInterval;
			}

			for (int i = 0; i < streamCount; i++)
			{
				var hitPoint = (Vector2)_results[i].transform.position + _randomHitPoint[UnityEngine.Random.Range(0, 8)];

				if (fire)
				{
					IPDS.CreateProjectile(
						_projectileName,
						hitPoint,
						Quaternion.identity,
						new ProjectileRpcParameter()
						{
							StartPosition = (Vector2)Muzzle.position,
							CollisionEvent = new CollisionEventStruct()
							{
								SenderId = ClientId,
								Effect = CollisionEffect.Hit,
								Damage = _damage
							},
							EffectColor = Color.red,
							LifeTime = 1f,
						});

					hitCount--;
				}
			}

			t += Time.deltaTime;
			fire = false;
			yield return null;
		}

		if (_remainderDelay != null)
		{
			yield return _remainderDelay;
		}

		_fireCo = null;
	}

	public void FireLaserStream(LaserPointData[] lpd, string buffName)
	{
		if (_laserCo != null)
		{
			StopCoroutine(_laserCo);
		}
		_laserCo = StartCoroutine(FireLaserStreamCoroutin(lpd, buffName));
	}

	IEnumerator FireLaserStreamCoroutin(LaserPointData[] lpd, string buffName)
	{
		bool burst = buffName == "burst";
		if (burst)
		{
			for (int i = 0; i < _maxLaserStreamCount; i++)
			{
				_laserHandlers[i].EffectColor = Color.magenta;
				_laserHandlers[i].UpdateColor();
			}
		}
		else
		{
			for (int i = 0; i < _maxLaserStreamCount; i++)
			{
				_laserHandlers[i].EffectColor = Color.green;
				_laserHandlers[i].UpdateColor();
			}
		}

		float time = _totalFiringInterval / 1000f;

		for (int i = 0; i < _maxLaserStreamCount; i++)
		{
			if (i < lpd.Length)
			{
				if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(lpd[i].EnemyNetworkObjectId, out var no))
				{
					_transformCache[i] = no.transform;
				}
				_laserHandlers[i].Show();
			}
			else
			{
				_laserHandlers[i].Stop();
			}
		}

		if (burst)
		{
			_ac.PlayAudio("laser-7", Muzzle.position);
		}
		else
		{
			_ac.PlayAudio("arc-laser", Muzzle.position);
		}

		while (time > 0f)
		{
			for (int i = 0; i < lpd.Length; i++)
			{
				if (!_laserHandlers[i].IsShow)
				{
					continue;
				}

				if (!_transformCache[i].gameObject.activeSelf)
				{
					_laserHandlers[i].Stop();
					continue;
				}
				_laserHandlers[i].StartPosition = Muzzle.position;
				_laserHandlers[i].EndPosition = (Vector2)_transformCache[i].position + lpd[i].LaserPointFromCenter;
			}

			time -= Time.deltaTime;
			yield return null;
		}

		for (int i = 0; i < lpd.Length; i++)
		{
			_laserHandlers[i].Stop();
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
		_fireCo = null;
		_laserCo = null;
	}
}
