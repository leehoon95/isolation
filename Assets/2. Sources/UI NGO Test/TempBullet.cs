using Unity.Netcode;
using UnityEngine;

public class TempBullet : NetworkBehaviour
{
	[SerializeField]
	Rigidbody2D _rigidbody;

	public override async void OnNetworkSpawn()
	{
		if (!IsOwner)
		{
			return;
		}
		
		await Awaitable.WaitForSecondsAsync(3f);

		//DespawnObjectRpc(NetworkObject);
	}

	void FixedUpdate()
	{
		//transform.Translate(Vector2.up * Time.deltaTime);
	}

	[Rpc(SendTo.Server)]
	void DespawnObjectRpc(NetworkObjectReference target)
	{
		if (target.TryGet(out NetworkObject targetObject))
		{
			targetObject.Despawn();
		}
	}
}
