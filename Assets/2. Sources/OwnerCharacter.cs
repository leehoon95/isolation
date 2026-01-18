using System;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class OwnerCharacter : NetworkBehaviour
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	TMP_Text _text;

	InputSystem _inputSystem;
	Vector2 _inputDirection;
	Vector2 _inputReceived;
	float _angle;
	PooledDynamicSpawner _dynamicSpawner;
	float _fireInterval = (60f / 600f) * 1000f;
	bool _attack;
	float _lastFiredTime;
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

	public override void OnNetworkSpawn()
	{
		//Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsHost: {IsHost}");
		//Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsClient: {IsClient}");
		//Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsOwner: {IsOwner}");
		if (IsOwner)
		{
			_inputSystem = FindAnyObjectByType<InputSystem>();

			if (_inputSystem == null)
			{
				return;
			}
			
			_inputSystem.Move += OnMove;
			//_inputSystem.Look += OnLook;
			_inputSystem.Attack += OnAttack;
			_inputSystem.Attack2 += OnAttack2;

			_dynamicSpawner = FindAnyObjectByType<PooledDynamicSpawner>();
			if (_dynamicSpawner == null)
			{
				Debug.LogWarning("OwnerCharacter.OnNetworkSpawn No found PooledDynamicSpawner");
			}
		}

		base.OnNetworkSpawn();
	}

	void OnMove(Vector2 dir)
	{
		_inputDirection = dir;
	}

	void OnLook(Vector2 pos)
	{
		Vector2 directionToMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
		_angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
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
		if (IsOwner)
		{
			OnLook(Mouse.current.position.ReadValue());

			if (_inputDirection.magnitude > float.Epsilon)
			{
				_rigidbody.linearDamping = 0f;
				_rigidbody.linearVelocity = _inputDirection * 5f;
			}
			else
			{
				_rigidbody.linearDamping = 30f;
			}

			long now = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
			if (_attack && (now - _lastFiredTime) > _fireInterval)
			{
				var worldUp = transform.TransformDirection(Vector3.up);

				var rq = Quaternion.Euler(0f, 0f, RandomNormal(0f, 5f));
				var dispersedUp = rq * worldUp;
				//var dispersedUp = Quaternion.AngleAxis(RandomNormal(0f, 2f), Vector3.forward) * worldUp;
				
				_dynamicSpawner.CreateObject(
					"bullet",
					transform.position + dispersedUp.normalized,
					transform.rotation * rq);
				}
		}
	}

	float RandomNormal(float mean, float stdDev)
	{
		float u1 = 1.0f - UnityEngine.Random.value; // (0,1] ¹üÀ§
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
			_inputSystem.Attack -= OnAttack;
			_inputSystem.Attack2 -= OnAttack2;
		}
	}
}
