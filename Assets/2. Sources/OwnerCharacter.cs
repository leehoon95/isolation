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
	TMP_Text _text2;
	[SerializeField]
	GameObject _triangle;
	[SerializeField]
	GameObject _bullet;

	InputSystem _inputSystem;
	int _pingCount;

	NetworkVariable<float> _angle = new NetworkVariable<float>(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner
		);

	public override void OnNetworkSpawn()
	{
		if (HasAuthority)
		{
			Debug.Log($"{NetworkManager.Singleton.LocalClientId} Has authority");
		}
		else
		{
			Debug.Log($"{NetworkManager.Singleton.LocalClientId} Has not authority");
		}


		Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsHost: {IsHost}");
		Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsClient: {IsClient}");
		Debug.Log($"OwnerCharacter.OnNetworkSpawn() IsOwner: {IsOwner}");

		//_text = GetComponentInChildren<TMP_Text>();
		_text.text = $"{OwnerClientId}";

		_inputSystem = FindAnyObjectByType<InputSystem>();

		if (_inputSystem == null)
		{
			print("OwnerCharacter. _inputSystem is null");
			return;
		}

		if (IsOwner)
		{
			Debug.Log($"{_text.text} is owner");
			
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

		base.OnNetworkSpawn();
	}

	public override void OnDestroy()
	{
		base.OnDestroy();

		if (IsOwner)
		{
			_inputSystem.Move -= OnMove;
			_inputSystem.Attack -= OnAttack;
			_inputSystem.Attack2 -= OnAttack2;
		}
	}

	void OnMove(Vector2 dir)
	{
		_rigidbody.linearVelocity = dir * 5f;
	}

	void OnAttack(bool performed)
	{
		if (performed)
		{
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
