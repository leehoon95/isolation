using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class OwnerCharacter : NetworkBehaviour
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	TMP_Text _text;
	[SerializeField]
	GameObject _gun;
	[SerializeField]
	SortingGroup _sortingGroup;

	InputSystem _inputSystem;
	Vector2 _inputDirection;
	Vector2 _inputReceived;
	float _angle;
	PooledDynamicSpawner _dynamicSpawner;
	//float _fireInterval = (200f / 600f) * 1000f;
	float _fireInterval = 50f;
	bool _attack;
	long _lastFiredTime;
	//NetworkVariable<float> _angle = new NetworkVariable<float>(
	//	0,
	//	NetworkVariableReadPermission.Everyone,
	//	NetworkVariableWritePermission.Owner
	//	);

	public Color BodyColor
	{
		set => _spriteRenderer.color = value;
	}

	public string BodyText
	{
		set => _text.text = value;
	}

	public Color BodyTextColor
	{
		set => _text.color = value;
	}

	public override void OnNetworkSpawn()
	{
		if (IsOwner)
		{
			_inputSystem = FindAnyObjectByType<InputSystem>();
			_dynamicSpawner = FindAnyObjectByType<PooledDynamicSpawner>();
			if (_inputSystem == null)
			{
				Debug.LogError("OwnerCharacter.OnNetworkSpawn No found essential objects\n" +
					$"InputSystem: {_inputSystem}, PDS: {_dynamicSpawner}");
				return;
			}

			
			if (_dynamicSpawner == null)
			{
				
			}

			_inputSystem.Move += OnMove;
			//_inputSystem.Attack += OnAttack;
			//_inputSystem.Attack2 += OnAttack2;



			_sortingGroup.sortingOrder = 1;
		}

		base.OnNetworkSpawn();
	}

	void OnMove(Vector2 dir)
	{
		_inputDirection = dir;
	}

	void OnAttack(bool performed)
	{
		if (performed)
		{

			_attack = true;
		}
		else
		{
			_attack = false;
		}
	}

	void OnAttack2(bool performed)
	{
		if (performed)
		{
		}
		else
		{
		}
	}

	void Update()
	{
		if (IsOwner)
		{
			transform.rotation = Quaternion.Euler(0f, 0f, _angle - 90f);
		}
	}

	void FixedUpdate()
	{
		//if (IsOwner)
		//{
		//	// player->mouse direction
		//	var mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		//	mousePosition.z = 0f;
		//	var distance = (mousePosition - transform.position).magnitude;

		//	var gunX = _gun.transform.localPosition.x;
		//	var gunTargetY = Mathf.Sqrt(distance * distance - gunX * gunX);

		//	var gunCorrectionAngle = Vector2.Angle(
		//		new Vector2(gunX, gunTargetY),
		//		new Vector2(0f, distance));

		//	GLogger.Log($"{gunX} {gunTargetY} {distance} {gunCorrectionAngle}");

		//	/*
		//	 * 마우스가 캐릭터 중심과 gun 사이에 있으면 계산 불가
		//	 */
		//	if (distance > _gun.transform.localPosition.magnitude)
		//	{
		//		//var toMouseFromGun = mousePosition - gunWorldPosition;
		//		var toMouse = mousePosition - transform.position;
		//		_angle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg + gunCorrectionAngle;
		//	}

		//	if (_inputDirection.magnitude > float.Epsilon)
		//	{
		//		_rigidbody.linearDamping = 0f;
		//		_rigidbody.linearVelocity = _inputDirection * 5f;
		//	}
		//	else
		//	{
		//		_rigidbody.linearDamping = 30f;
		//	}

		//	long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
		//	if (_attack && (now - _lastFiredTime) > _fireInterval)
		//	{
		//		var worldUp = transform.TransformDirection(Vector3.up);

		//		var rq = Quaternion.Euler(0f, 0f, RandomNormal(0f, 1f));
		//		var spreadUp = rq * worldUp;

		//		_dynamicSpawner?.CreateProjectile(
		//			"BulletNormal",
		//			//transform.position + spreadUp.normalized,
		//			_gun.transform.position,
		//			transform.rotation * rq,
		//			new ProjectileRpcParameter()
		//			{
		//				Speed = 10f,
		//				//CollisionMask = LayerMask.GetMask("Enemy", "StaticObject"),
		//				//CollisionEffect = (int)CollisionEffect.Damage,
		//				//CollisionEffectDetail = "",
		//				EffectColor = _text.color,
		//				LifeTime = 1f
		//			});
		//		_dynamicSpawner?.CreateEffect(
		//			"EffectHitNormal",
		//			//transform.position + spreadUp.normalized,
		//			_gun.transform.position,
		//			transform.rotation * rq,
		//			new EffectRpcParameter());
		//		_lastFiredTime = now;
		//	}
		//}
	}

	float RandomNormal(float mean, float stdDev)
	{
		float u1 = 1.0f - UnityEngine.Random.value; // (0,1] 범위
		float u2 = 1.0f - UnityEngine.Random.value;
		float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);
		return mean + stdDev * randStdNormal;
	}

	//[Rpc(SendTo.Server)]
	//void SetInputDataRpc(Vector2 inputFromClient)
	//{
	//	Debug.Log("OwnerCharacter.SetInputDataRpc");
	//	_inputReceived = inputFromClient;
	//}

	public override void OnDestroy()
	{
		base.OnDestroy();

		if (IsOwner)
		{
			_inputSystem.Move -= OnMove;
			//_inputSystem.Look += OnLook;
			//_inputSystem.Attack -= OnAttack;
			//_inputSystem.Attack2 -= OnAttack2;
		}
	}
}
