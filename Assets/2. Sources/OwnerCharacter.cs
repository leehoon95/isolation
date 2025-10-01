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
	[SerializeField]
	NetworkObject _bullet;

	InputSystem _inputSystem;
	bool _attack1, _attack2;
	int _pingCount;

	NetworkVariable<float> _angle = new NetworkVariable<float>(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner
		);

	public override void OnNetworkSpawn()
	{
		base.OnNetworkSpawn();

		print("OwnerCharacter.OnNetworkSpawn() called");

		_text.text = OwnerClientId.ToString();

		_inputSystem = FindAnyObjectByType<InputSystem>();

		if (_inputSystem == null)
		{
			print("OwnerCharacter. _inputSystem is null");
			return;
		}


		if (IsLocalPlayer)
		{
			print("OwnerCharacter. IsLocalPlayer.");

			_inputSystem.Move += OnMove;
			_inputSystem.Attack += OnAttack;
			_inputSystem.Attack2 += OnAttack2;
		}

		if (IsOwnedByServer)
		{
			_spriteRenderer.color = Color.green;
		}
		else
		{
			_spriteRenderer.color = Color.red;
		}
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		_inputSystem.Move -= OnMove;
		_inputSystem.Attack -= OnAttack;
	}

	void OnMove(Vector2 dir)
	{
		_rigidbody.linearVelocity = dir * 5f;
	}

	void OnAttack(bool performed)
	{
		if (performed)
		{
			_attack1 = true;
		}
		else
		{
			_attack1 = false;
		}
	}

	void OnAttack2(bool performed)
	{
		if (performed)
		{
			_attack2 = true;
		}
		else
		{
			_attack2 = false;
		}
	}
	
	void FixedUpdate()
	{
		if (IsOwner)
		{
			Vector2 directionToMouse = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue()) - transform.position;
			_angle.Value = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
			
			transform.rotation = Quaternion.Euler(0f, 0f, _angle.Value - 90f);
		}
		else
		{
			transform.rotation = Quaternion.Euler(0f, 0f, _angle.Value - 90f);
		}
	}

	[Rpc(SendTo.Server)]
	void SpawnBulletRpc()
	{
		var obj = Instantiate(_bullet, default, Quaternion.identity);

		obj.GetComponent<NetworkObject>()
			.Spawn(false);
	}
}
