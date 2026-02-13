using Mono.Cecil;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class MovingWall : NetworkBehaviour, ICollisionInteractable
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	TMP_Text _text;

	NetworkVariable<int> count = new(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Server
		);
	float _time;

	public override void OnNetworkSpawn()
	{
		count.OnValueChanged += (previoudValue, newValue) => { _text.text = $"{newValue}"; };
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
		ProcessCollisionEffectgRpc((int)ce.Effect, ce.EffectDetail);
	}

	public CollisionEffect GetEffect()
	{
		return CollisionEffect.None;
	}

	void FixedUpdate()
	{
		if (!IsServer)
		{
			return;
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
	void ProcessCollisionEffectgRpc(int effect, FixedString32Bytes effectDetail, RpcParams rpcParams = default)
	{
		//GLogger.Log($"{rpcParams.Receive.SenderClientId} hit wall");
		if (effect == (int)CollisionEffect.Damage)
		{
			count.Value = count.Value + int.Parse(effectDetail.ToString());
		}
	}
}
