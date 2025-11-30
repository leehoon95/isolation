using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;

/*
 * In-scene placed NetworkObjects
 * 풀 관리 시스템
 * 생성하는 것보다 배치하는 것이 쉽거나 상호작용 가능한 오브젝트
 * 아이템이나 플레이어 정보가 포함된 HUD
 * 플레이어가 트리거에 진입하거나 위치를 특정할 수 있는 텔레포터
 * NetworkManager에 등록이 필요하지 않음
 * Scene Management가 활성화되면 추적 및 식별 목적으로 내부적으로 등록됨
 * 
 * 획득 가능한 아이템은 일반적으로 배치와 동적 생성된 네트워크 객체를 모두 사용하는 하이브리드 방식이 좋음
 * 장면 내에 배치된 것은 아이템에 대한 추가 정보
 * 동적 생성된 객체는 아이템 자체
 * 
 * 공톡적으로 Awake가 먼저
 * ->
 * In-scene placed:		Start	->	OnNetworkSpawn
 * Dynamically spawned:	OnNetworkSpawn	->	Start
 * 
 * 동적으로 스폰된 NetworkObjects에서 DontDestroyOnLoad는 사용하지 마라
 */


/*
 * host/client 공용
 * 
 * 
 * 
 */
public class PlayerSpawner : NetworkBehaviour, INetworkPrefabInstanceHandler
{
	[SerializeField]
	GameObject _playerPrefab;
	[SerializeField]
	GameObject _otherPlayerPrefab;

	// client id, player object
	Dictionary<ulong, NetworkObject> _clientInstances = new();

	// 소유한 player의 instance
	//GameObject _instance;
	//NetworkObject _instanceNO;
	
	public override void OnNetworkSpawn()
	{
		Debug.Log($"PlayerSpawner.OnNetworkSpawn	owner: {OwnerClientId}	Authority: {HasAuthority}");

		/*
		 * host only!
		 * host는 client이면서 server로 간주되므로 override를 수동으로 등록해야 한다
		 * sourceNetworkPrefab: 오버라이드 대상
		 * networkPrefabOverrides: 하나 이상의 오버라이드로 사용되는 프리펩
		 */
		if (!IsHost)
		{
			Debug.Log("PlayerSpawner.OnNetworkSpawn AddHandler called");

			NetworkManager.PrefabHandler.AddHandler(_playerPrefab, this);
			NetworkManager.PrefabHandler.AddHandler(_otherPlayerPrefab, this);
		}
	}
	
	/*
	 * INetworkprefabInstanceHandler.Instantiate 구현
	 * Instantiate 메서드는 권한이 없는 client에서만 호출됨
	 * 권한에 대한 네트워크 프리팹 동작을 지정하려면 prefab override를 사용
	 * 
	 * Authority(권한)에서 다른 클라이언트와 다른 prefab instance를 사용하려면
	 * prefab override를 고려한다.
	 * 
	 */
	public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
	{
		//Debug.Log($"PlayerSpawner.Instantiate	client id: {ownerClientId} {position} {rotation.eulerAngles}");
		if (_clientInstances.TryGetValue(ownerClientId, out var p))
		{
			p.gameObject.SetActive(true);

			return p;
		}
		else
		{
			GameObject obj = null;

			if (NetworkManager.LocalClientId == ownerClientId)
			{
				obj = Instantiate(_playerPrefab);
			}
			else
			{
				obj = Instantiate(_otherPlayerPrefab);
			}

			obj.transform.SetPositionAndRotation(position, rotation);

			var oc = obj.GetComponent<OwnerCharacter>();

			SetPlayerShape(ownerClientId, oc);

			return obj.GetComponent<NetworkObject>();
		}
	}

	/*
	 * INetworkprefabInstanceHandler.Destroy 구현
	 * 모든 클라이언트에서 호출됨
	 */
	public void Destroy(NetworkObject networkObject)
	{
		//Debug.Log("PlayerSpawner.Destroy");
		if (_clientInstances.TryGetValue(networkObject.NetworkObjectId, out var p))
		{
			p.gameObject.SetActive(false);
		}
		//if (IsHost)
		//{

		//}
		//else
		//{
		//	if (_instance != null)
		//	{
		//		_instance.SetActive(false);
		//	}
		//}
	}

	public void Spawn()
	{
		if (IsHost)
		{
			SpawnPlayer(NetworkManager.LocalClientId);
		}
		else
		{
			SpawnObjectRpc();
		}
	}

	[Rpc(SendTo.Server)]
	void SpawnObjectRpc(RpcParams rpcParams = default)
	{
		SpawnPlayer(rpcParams.Receive.SenderClientId);
	}

	/*
	 * 명시적으로 owner id를 지정하지 않을 경우 authority을 가진 client(host)가 owner가 된다
	 * clientId: spawn 대상의 owner client id
	 */
	void SpawnPlayer(ulong clientId, bool destroyWithScene = true)
	{
		if (!HasAuthority)
		{
			//Debug.LogWarning("PlayerSpawner.SpawnObject You don't have the authority!");
			return;
		}

		if (_clientInstances.TryGetValue(clientId, out var p))
		{
			//Debug.Log("PlayerSpawner.SpawnPlaeyer found client object");
			p.gameObject.SetActive(true);
			p.SpawnWithOwnership(clientId, destroyWithScene);
		}
		else
		{
			GameObject obj = null;
			if (NetworkManager.LocalClientId == clientId)
			{
				obj = Instantiate(_playerPrefab);
			}
			else
			{
				obj = Instantiate(_otherPlayerPrefab);
			}

			SetPlayerShape(clientId, obj.GetComponent<OwnerCharacter>());
			var nobj = obj.GetComponent<NetworkObject>();
			nobj.SpawnWithOwnership(clientId, destroyWithScene);
			
			NotifyPlayerSpawnedRpc(clientId, nobj.NetworkObjectId);
		}

		//if (isHost)
		//{
		//	if (_clientInstances.TryGetValue(clientId, out var item))
		//	{
		//		item.Spawn();
		//		item.gameObject.SetActive(true);
		//	}
		//	else
		//	{
		//		var obj = Instantiate(_otherPlayerPrefab);
		//		obj.GetComponent<NetworkObject>().SpawnWithOwnership(clientId, destroyWithScene);
		//		SetPlayerShape(clientId, obj.GetComponent<OwnerCharacter>());
		//		_clientInstances[clientId] = obj.GetComponent<NetworkObject>();
		//	}
		//}
		//else
		//{

		//	var obj = Instantiate(_playerPrefab);

		//	obj.GetComponent<NetworkObject>().Spawn(destroyWithScene);

		//	_instance = obj;
		//	_instanceNO = _instance.GetComponent<NetworkObject>();
		//	_instance.SetActive(true);

		//	SetPlayerShape(NetworkManager.LocalClientId, obj.GetComponent<OwnerCharacter>());
		//}
	}

	public void Despawn()
	{
		if (IsHost)
		{
			DespawnPlayer(NetworkManager.LocalClientId);
		}
		else
		{
			DespawnObjectRpc();
		}
	}

	[Rpc(SendTo.Server)]
	void DespawnObjectRpc(RpcParams rpcParams = default)
	{
		DespawnPlayer(rpcParams.Receive.SenderClientId);
	}

	void DespawnPlayer(ulong clientId)
	{
		//Debug.Log($"PlayerSpawner.Despawn clientId: {clientId}");
		if (_clientInstances.TryGetValue(clientId, out var p))
		{
			p.Despawn(false);
			p.gameObject.SetActive(false);
		}
	}
	
	/*
	 * clientId 소유의 player가 새로 spawned됨을 알림(같은 player object가 두 번 spawn 되어도 호출되지 않음)
	 */
	[Rpc(SendTo.Everyone)]
	void NotifyPlayerSpawnedRpc(ulong clientId, ulong spawnedObjectId, RpcParams rpcParams = default)
	{
		//Debug.Log($"PlayerSpawner.NotifyPlayerSpawned {clientId} {spawnedObjectId} {rpcParams.Receive.SenderClientId}");
		if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(spawnedObjectId, out var obj))
		{
			_clientInstances[clientId] = obj;
		}
		else
		{
			Debug.LogWarning($"PlayerSpawner.NotifyPlayerSpawned No found object {spawnedObjectId}");
		}
	}

	void SetPlayerShape(ulong ownerClientId, OwnerCharacter oc)
	{
		//Debug.Log("PlayerSpawner.SetPlayerShape");
		if (NetworkManager.ServerClientId == ownerClientId)
		{
			oc.BodyColor = Color.red;
			oc.BodyText = $"H";
		}
		else
		{
			oc.BodyColor = Color.green;
			oc.BodyText = $"{ownerClientId}";
		}
	}
}