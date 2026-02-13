using System;
using Unity.Netcode;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerBase : NetworkBehaviour
{
	InputSystem _inputSystem;
	PooledDynamicSpawner _pds;


	public PooledDynamicSpawner PDS { get => _pds; }
	public bool LeftTrigger { get; private set; }
	public bool RightTrigger { get; private set; }
	public Vector2 MovementValue { get; private set; }

	protected InputSystem PlayerInput => _inputSystem;

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

		_inputSystem.Move += OnMove;
		_inputSystem.LeftTrigger += OnLeftTrigger;
		_inputSystem.RightTrigger += OnRightTrigger;
	}

	public override void OnNetworkDespawn()
	{
		if (!IsOwner)
		{
			return;
		}

		_inputSystem.Move -= OnMove;
		_inputSystem.LeftTrigger -= OnLeftTrigger;
		_inputSystem.RightTrigger -= OnRightTrigger;
	}


	void OnMove(Vector2 value) => MovementValue = value;
	void OnLeftTrigger(bool trigger) => LeftTrigger = trigger;
	void OnRightTrigger(bool trigger) => RightTrigger = trigger;
}
