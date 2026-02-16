using System;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBase : NetworkBehaviour
{
	InputSystem _inputSystem;
	PooledDynamicSpawner _pds;


	protected InputSystem PlayerInput => _inputSystem;
	protected PooledDynamicSpawner PDS => _pds;


	public override void OnNetworkSpawn()
	{
		if (!IsOwner)
		{
			return;
		}

		_inputSystem = FindAnyObjectByType<InputSystem>();
		_pds = FindAnyObjectByType<PooledDynamicSpawner>();

		if (_inputSystem == null
			|| _pds == null)
		{
			throw new NullReferenceException($"Check null reference"
				+ $"input system is {_inputSystem}\n"
				+ $"pooled dynamic spawner is {_pds}");
		}
	}

	public override void OnNetworkDespawn()
	{
		if (!IsOwner)
		{
			return;
		}
	}
}
