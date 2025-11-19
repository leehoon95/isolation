using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Unity.Multiplayer.Center.NetcodeForGameObjectsExample.DistributedAuthority;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

/*
 * Pool에 들어가는 object interface
 * PrefabId: 어떤 prefab에서 Instantiate되었는 알 수 있는 값
 * ObjectId: (client id)_(local에서 중복되지 않는 값) => 로컬, 네트워크에서 중복되지 않음
 * SO: prefab 생성시 연결할 ScriptableObject. 생성된 instance는 이 SO를 변경하면 안 됨.
 */
public interface IDynamicPooledObject
{
	string PrefabId { get; set; }
	string ObjectId { get; set; }
	ulong ClientId { get; set; }
	ScriptableObject SO { get; set; }
	IPooledDynamicSpawner Spawner { set; }
	IDynamicPooledObject DPO { get; }
	GameObject GO { get; }
	NetworkObject NO { get; }
	bool OnlyInteractInOwnerClient { get; set; }
	void SetActive(bool active);
	void SetTransform(Vector2 position, Quaternion rotation);
	void Clean();
}

/*
 * Pooled item에서 pool을 참조 목적용 
 */
public interface IPooledDynamicSpawner
{
	void ReleaseObject(IDynamicPooledObject obj);
	//void Despawn(IDynamicPooledObject go);
}

/*
 * scene에 GameObject 생성(또는 spawn)하는 pool
 * 외부에서 identifier(string)으로 prefab을 생성 요청
 */
public class PooledDynamicSpawner : NetworkBehaviour, IPooledDynamicSpawner
{
	[SerializeField]
	List<PoolConfig> _poolConfig;

	/*
	 * pool 설정값이며 1:1 대응
	 * PrefabId: prefab 종류. 네트워크에서 어떤 prefab인지 알기 위해 사용함
	 * SO: object 생성시에만 사용되고, 이미 생성된 object에는 적용되지 않음
	 */
	[Serializable]
	public class PoolConfig
	{
		public string PrefabId;
		public GameObject Prefab;
		public int Capacity;
		public bool OnlyInteractInOwnerClient;
		public ScriptableObject SO;
	}

	/*
	 * string: identifier. PoolConfig.Identifier와 같은 값
	 */
	Dictionary<string, PoolConfig> _configs = new();

	/*
	 * string: identifier
	 */
	Dictionary<string, ObjectPool<IDynamicPooledObject>> _pools = new(); // prefab identifier(string), pool
	
	/*
	 * string: guid
	 */
	Dictionary<string, IDynamicPooledObject> _activatedObjects = new(); // guid, GameObject

	/*
	 * 네트워크에서 사용되는 identifier 일부의 값을 담당하는 pool
	 */
	HashSet<string> _objectIdPool = new HashSet<string>();
	uint _objectIdCounter;

	public override void OnNetworkSpawn()
	{
		if (_poolConfig.Count > 0)
		{
			int index = 0;
			while (_poolConfig.Count > index)
			{
				var poolConfig = _poolConfig[index];
				//var key = poolConfig.Prefab.GetComponent<NetworkObject>().PrefabIdHash;
				var prefabId = poolConfig.PrefabId;
				var pool = new ObjectPool<IDynamicPooledObject>(
				createFunc: () =>
				{
					var obj = GameObject.Instantiate(poolConfig.Prefab);
					var instance = obj.GetComponent<IDynamicPooledObject>();
					
#if UNITY_EDITOR
					if (instance == null)
					{
						throw new NullReferenceException("No IDynamicPooledObject component fonded");
					}
#endif
					//instance.SetActive(false);

					return instance;
				},
				actionOnGet: (instance) =>
				{
					instance.GO.SetActive(true);
				},
				actionOnRelease: (instance) =>
				{
					instance.GO.SetActive(false);
				},
				actionOnDestroy: (instance) =>
				{
					GameObject.Destroy(instance.GO);
				},
				collectionCheck: true,
				defaultCapacity: 10);

				if (_configs.ContainsKey(prefabId) || _pools.ContainsKey(prefabId))
				{
					Debug.LogWarning($"PooledDynamicSpawner.OnNetworkSpawn This prefabId({prefabId}) is duplicated");
				}

				_configs[prefabId] = poolConfig;
				_pools[prefabId] = pool;
				index++;
			}
		}

		base.OnNetworkSpawn();
	}

	public PoolConfig GetConfig(string identifier)
	{
		try
		{
			return _configs[identifier];
		}
		catch (KeyNotFoundException e)
		{
			Debug.LogException(e);
			return null;
		}
	}

	/*
	 * 다른 클라이언트에게 effect만 보여줄 때 사용
	 */
	public void CreateObject(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		bool destroyWithScene = true)
	{
		string objectId;
		lock(_objectIdPool)
		{
			objectId = $"{NetworkManager.LocalClientId}_{_objectIdCounter}";
			_objectIdPool.Add(objectId);
			_objectIdCounter++;
		}

		//CreateObjectImplementation(prefabId, objectId, position, rotation);
		CreateObejctWithAnotherClientRpc(NetworkManager.Singleton.LocalClientId, prefabId, objectId, position, rotation);
	}

	/*
	 * obejct를 생성. 다른 client에게는 effect만 보일 수 있다
	 */
	[Rpc(SendTo.Everyone)]
	void CreateObejctWithAnotherClientRpc(
		ulong clientId,
		string prefabId,
		string objectId,
		Vector2 position,
		Quaternion rotation,
		bool destroyWithScene = true,
		RpcParams rpcParams = default)
	{
		CreateObjectImplementation(clientId, prefabId, objectId, position, rotation);
	}

	/*
	 * (Unreliable 버전)
	 */
	//[Rpc(SendTo.NotMe, Delivery = RpcDelivery.Unreliable)]
	//void CreateObejctWithAnotherClientUnreliableRpc(
	//	string prefabIdHash,
	//	string guid,
	//	Vector2 pos,
	//	Quaternion rotation,
	//	bool destroyWithScene = true,
	//	RpcParams rpcParams = default)
	//{
	//	CreateObjectImplementation(prefabIdHash, guid, pos, rotation);
	//}

	void CreateObjectImplementation(
		ulong clientId,
		string prefabId,
		string objectId,
		Vector2 position, 
		Quaternion rotation)
	{
		// prefab 찾기
		if (!_pools.TryGetValue(prefabId, out var pool))
		{
			Debug.LogWarning($"PooledDynamicSpawner.CreateObject unknown prefab{prefabId}");
			return;
		}

		IDynamicPooledObject dpo;
		lock (pool)
		{
			dpo = pool.Get();
		}

		dpo.ClientId = clientId;
		dpo.PrefabId = prefabId;
		dpo.ObjectId = objectId;
		dpo.Spawner = this;
		dpo.OnlyInteractInOwnerClient = true;
		dpo.SO = _configs[prefabId].SO;
		dpo.SetTransform(position, rotation);
		dpo.SetActive(true);

		lock (_activatedObjects)
		{
			_activatedObjects[objectId] = dpo;
		}
	}

	/*
	 * object를 spawn
	 */
	public void SpawnObjectWithOwnership(
		Vector2 pos,
		Quaternion rotation,
		bool destroyWithScene = true)
	{
		//
	}

	/*
	 * obejct를 spawn
	 */
	[Rpc(SendTo.Server)]
	void SpawnObejctaRpc(
		Vector2 pos,
		Quaternion rotation,
		bool withOwnership,
		ulong clientId,
		bool destroyWithScene = true,
		RpcParams rpcParams = default)
	{
		Debug.Log("PooledDynamicSpawner.CreateObejctOnAnotherClientRpc");
		//GameObject obj = null;

		//lock (this)
		//{
		//	obj = _pool.Get();
		//}

		//NetworkObject no = obj.GetComponent<NetworkObject>();

		//obj.transform.SetPositionAndRotation(pos, rotation);
	}

	void Clean()
	{
		lock (_pools)
		{
			_pools.Clear();
		}
	}

	/*
	 * 일반 GameObject를 사용하는 경우
	 */
	public void ReleaseObject(IDynamicPooledObject dpo)
	{
		//ReleaseObjectImplementation(dpo.PrefabId, dpo.ObjectId);
		ReleaseObjectRpc(dpo.PrefabId, dpo.ObjectId);
	}

	[Rpc(SendTo.Everyone)]
	void ReleaseObjectRpc(string prefabId, string objectId)
	{
		ReleaseObjectImplementation(prefabId, objectId);
	}

	void ReleaseObjectImplementation(string prefabId, string objectId)
	{
		IDynamicPooledObject dpo;
		lock(_activatedObjects)
		{
			Debug.Log($"release {objectId}");
			if (_activatedObjects.TryGetValue(objectId, out var obj))
			{
				dpo = obj;
			}
			else
			{
				Debug.LogWarning($"PooledDynamicSpawner.ReleaseObjectImplement no activated object found({objectId})");
				return;
			}

			_activatedObjects.Remove(objectId);
		}

		lock (_pools)
		{
			if (_pools.TryGetValue(prefabId, out var pool))
			{
				try
				{
					pool.Release(dpo);
				}
				catch(InvalidOperationException e)
				{
					Debug.LogException(e);
				}
			}
			else
			{
				Debug.LogWarning($"PooledDynamicSpawner.ReleaseObjectImplement unknown pool({prefabId})");
			}
		}

		lock(_objectIdPool)
		{
			_objectIdPool.Remove(prefabId);
		}
	}

	/*
	 * NetworkObject를 사용하는 경우
	 */
	public void Despawn(IDynamicPooledObject go)
	{
		//lock (_pool)
		//{
		//	_pool
		//}
	}
}