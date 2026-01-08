using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;


/*
 * scene에 GameObject 생성(또는 spawn)하는 pool
 * 외부에서 objectId(string)으로 prefab을 생성 요청
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
				var prefabId = poolConfig.PrefabId;
				var pool = new UnityEngine.Pool.ObjectPool<IDynamicPooledObject>(
				createFunc: () =>
				{
					var obj = GameObject.Instantiate(poolConfig.Prefab);
					var pdo = obj.GetComponent<IDynamicPooledObject>();
					
#if UNITY_EDITOR
					if (pdo == null)
					{
						throw new NullReferenceException("No IDynamicPooledObject component fonded");
					}
#endif

					return pdo;
				},
				actionOnGet: (instance) =>
				{

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

		dpo.GO.SetActive(true);
		dpo.ClientId = clientId;
		dpo.PrefabId = prefabId;
		dpo.ObjectId = objectId;
		dpo.Spawner = this;
		dpo.OnlyInteractInOwnerClient = true;
		dpo.SO = _configs[prefabId].SO;
		dpo.SetTransform(position, rotation);
		dpo.SetLifeTime(true, 2f);

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
			_objectIdPool.Remove(objectId);
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