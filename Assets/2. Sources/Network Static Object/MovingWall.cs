using Mono.Cecil;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MovingWall : NetworkBehaviour, INetworkObjectCollision
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	TMP_Text _text;

	NetworkVariable<int> _count = new(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Server
		);
	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEventCache = new()
	{
		Effect = CollisionEffect.None
	};
	float _time;

	public override void OnNetworkSpawn()
	{
		if (!IsHost)
		{
			return;
		}

		_collisionEventList = new();
		_collisionEventCache = new()
		{
			Effect = CollisionEffect.None
		};
		_count.OnValueChanged += (previoudValue, newValue) => { _text.text = $"{newValue}"; };
	}

	void FixedUpdate()
	{
		if (!IsHost)
		{
			return;
		}

		while (_collisionEventList.Count > 0)
		{
			var ce = _collisionEventList[0];
			_collisionEventList.RemoveAt(0);

			_count.Value += ce.Damage;
		}

		if (_time < 1f)
		{
			_rigidbody.MovePosition(new Vector2(_time, 3f));
			
		}
		else if (_time > 1f && _time < 2f)
		{
			_rigidbody.MovePosition(new Vector2(2f - _time, 3f));
		}
		else
		{
			_time = 0f;
		}
		_time += Time.fixedDeltaTime;
	}

	[Rpc(SendTo.Server)]
	public void SendCollisionEventRpc(CollisionEventStruct ce)
	{
		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ce));
	}

	public void SendCollisionEvent(CollisionEvent ce)
	{
		SendCollisionEventRpc(ce);
	}
	public CollisionEvent GetCollisionEvent()
	{
		return _collisionEventCache;
	}

}
