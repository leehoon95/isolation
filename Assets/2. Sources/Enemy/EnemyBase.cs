using System.Collections;
using Unity.Netcode;
using UnityEngine;

/*
 * Enemy와 Player가 충돌했을 때 충돌 이벤트를 판단하고 전달하는 주체는 client다 (client의 scene에서 결정한다)
 * 따라서 Enemy->Player Object 방향으로 이벤트를 전하지 않는다
 * Player가 이벤트를 가져가고 Enemy에게 이벤트를 전달한다
 */
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBase : NetworkBehaviour, IEnemyHandler
{
	Rigidbody2D _rigidbody;
	NetworkVariable<int> _healthPoint = new(
		readPerm: NetworkVariableReadPermission.Everyone,
		writePerm: NetworkVariableWritePermission.Server);
	Coroutine _effectInProgress;
	Transform _target;

	public int HealthPoint
	{
		get => _healthPoint.Value;
		set
		{
			_healthPoint.Value = Mathf.Max(0, value);
		}
	}

	public bool IsEffectInProgress
	{
		get => _effectInProgress != null;
	}

	// IEnemyHandler 구현
	public string PrefabId { get; set; }
	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IEnemySpawner Spawner { get; set; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public int MaxHealthPoint { get; set; }
	public float Speed { get; set; }
	public float KnockbackResistance { get; set; }
	public float StoppingPowerResistance { get; set; }
	public int Defense { get; set; }
	public Transform Target 
	{
		get => _target;
		set => _target = value;
	}

	[Rpc(SendTo.Server)]
	public void DespawnEnemyRpc()
	{
		NetworkObject.Despawn();
	}

	void Start()
	{
		if (_rigidbody == null)
		{
			_rigidbody = GetComponent<Rigidbody2D>();
		}
	}

	public override void OnNetworkSpawn()
	{
		Spawner.NotifyEnemySpawned(this);
	}

	public override void OnNetworkDespawn()
	{
		Spawner.NotifyEnemyDespawned(this);

		if (_effectInProgress != null)
		{
			StopCoroutine(_effectInProgress);
			_effectInProgress = null;
		}
	}

	protected void MoveToTarget(Vector2 targetPosition)
	{
		var direction = (targetPosition - (Vector2)transform.position).normalized;
		var desiredVelocity = direction * Speed;
		var steer = desiredVelocity - _rigidbody.linearVelocity;
		//GLogger.Log($"MoveToTarget {transform.position} to {targetPosition}");
		_rigidbody.AddForce(steer * 10f);
	}

	public void ApplyKnockback(Vector2 directionm, float knockbackForce, float knockbackTime)
	{
		if (_effectInProgress != null)
		{
			StopCoroutine(_effectInProgress);
		}

		_effectInProgress = StartCoroutine(Knockback(directionm, knockbackForce, knockbackTime));
	}

	IEnumerator Knockback(Vector2 direction, float knockbackForce, float knockbackTime)
	{
		_rigidbody.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
		yield return new WaitForSeconds(knockbackTime);
		_effectInProgress = null;
	}

	public void ApplyStopping(float stoppingTime)
	{
		if (_effectInProgress != null)
		{
			StopCoroutine(Stopping(stoppingTime));
		}

		_effectInProgress = StartCoroutine(Stopping(stoppingTime));
	}

	IEnumerator Stopping(float stoppingTime)
	{
		_rigidbody.linearVelocity = Vector2.zero;
		yield return new WaitForSeconds(stoppingTime);
		_effectInProgress = null;
	}

	/*
	 * Spawn되기 전에 호출된다.
	 * Host만 호출한다(EnemyPrefabWithDataHandler 참고)
	 */
	public virtual void SetData(in EnemyInstantiateData data)
	{
		PrefabId = data.PrefabId;
		Speed = data.Speed;
		MaxHealthPoint = data.MaxHealthPoint;
		KnockbackResistance = data.KnockbackResistance;
		StoppingPowerResistance = data.StoppingPowerResistance;
		Defense = data.Defense;
	}
}
