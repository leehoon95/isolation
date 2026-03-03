using System.Collections;
using System.Collections.Generic;
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
	bool _isEffectInProgress;
	Coroutine _effectInProgress;
	Transform _target;

	public int HealthPoint
	{
		get => _healthPoint.Value;
		set => _healthPoint.Value = Mathf.Clamp(value, 0, 100);
	}

	public bool IsEffectInProgress
	{
		get => _isEffectInProgress;
		private set => _isEffectInProgress = value;
	}

	// IEnemyHandler 구현
	public string PrefabId { get; set; }
	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IEnemySpawner Spawner { get; set; }

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

	public override void OnNetworkPreDespawn()
	{
		if (!IsHost)
		{
			return;
		}

		HealthPoint = MaxHealthPoint;
	}

	public override void OnNetworkSpawn()
	{
		if (!IsServer)
		{
			return;
		}
		_rigidbody = GetComponent<Rigidbody2D>();
		_healthPoint.Value = MaxHealthPoint;
	}

	//protected void MoveToTarget(Vector2 targetPosition)
	//{
	//	var direction = (targetPosition - (Vector2)transform.position).normalized;
	//	_rigidbody.linearVelocity = direction * 2f;
	//}

	public void ApplyKnockback(Vector2 directionm, float knockbackForce, float knockbackTime)
	{
		if (_effectInProgress != null)
		{
			StopCoroutine(_effectInProgress);
		}

		StartCoroutine(Knockback(directionm, knockbackForce, knockbackTime));
	}

	IEnumerator Knockback(Vector2 direction, float knockbackForce, float knockbackTime)
	{
		IsEffectInProgress = true;

		_rigidbody.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
		yield return new WaitForSeconds(knockbackTime);

		IsEffectInProgress = false;
		_effectInProgress = null;
	}

	public void ApplyStopping(float stoppingTime)
	{
		if (_effectInProgress != null)
		{
			StopCoroutine(Stopping(stoppingTime));
		}

		StartCoroutine(Stopping(stoppingTime));
	}

	IEnumerator Stopping(float stoppingTime)
	{
		IsEffectInProgress = true;

		_rigidbody.linearVelocity = Vector2.zero;
		yield return new WaitForSeconds(stoppingTime);

		IsEffectInProgress = false;
		_effectInProgress = null;
	}

	/*
	 * OnNetworkPreSpawn, OnNetworkSpawn 메서드가 호출되기 전에 먼저 호출된다.
	 */
	public virtual void SetData(in EnemyInstantiateData data)
	{
		if (IsServer)
		{

			PrefabId = data.PrefabId;
			Speed = data.Speed;
			MaxHealthPoint = data.MaxHealthPoint;
			KnockbackResistance = data.KnockbackResistance;
			StoppingPowerResistance = data.StoppingPowerResistance;
			Defense = data.Defense;


		}

	}
}
