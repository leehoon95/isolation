using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using UnityEngine.UIElements;

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
			_inputSystem.Look += OnLook;
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
			_dynamicSpawner.CreateObject(
				"bullet", 
				transform.position + transform.up, 
				transform.rotation);
		}
		else
		{
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
			if (_inputDirection.magnitude > float.Epsilon)
			{
				_rigidbody.linearDamping = 0f;
				_rigidbody.linearVelocity = _inputDirection * 5f;
			}
			else
			{
				_rigidbody.linearDamping = 30f;
			}
		}
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
			_inputSystem.Look += OnLook;
			_inputSystem.Attack -= OnAttack;
			_inputSystem.Attack2 -= OnAttack2;
		}
	}
}
