using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

/*
 * prefab 용도에 따라 PrefabId으로 구분하고 해당하는 pool을 생성하기 위한 설정클래스
 * 특수화를 위한 ScriptableObject또는 serialize 가능한 struct를 포함할 수 있다
 */
[Serializable]
public class PoolConfig
{
	[Tooltip("영대소문자, 숫자 최대 32자")]
	public string PrefabId;
	public GameObject Prefab;
	public int Capacity;
}


/*
 * scene에 GameObject 생성(또는 spawn)하는 pool
 * 외부에서 objectId(string)으로 prefab을 생성 요청
 * projectile 또는 effect를 생성한다
 */
public class PooledDynamicSpawner : NetworkBehaviour, IPooledDynamicSpawner
{
	[Header("pool을 설정할 수 있다(prefab 설정이 아님)")]
	[SerializeField]
	List<PoolConfig> _poolConfig;

	/*
	 * inspector에서 생성한 _poolConfig를 pool생성에 사용하고 빠름 검색을 위해 Dictionary 타입에 옮김
	 */
	Dictionary<string, PoolConfig> _configs = new();

	/*
	 * PoolConfig에 따라 ObjectPool을 보관
	 * prefapId으로 원하는 prefab을 관리하는 pool을 찾을 수 있다
	 */
	Dictionary<string, ObjectPool<IDynamicPooledObject>> _pools = new();
	
	/*
	 * 활성화된 object를 object id를 키로 보관
	 */
	Dictionary<string, IDynamicPooledObject> _activatedObjects = new();

	/*
	 * CreateObject 메서드에서 생성한 object를 관리
	 * 생성과 삭제는 이벤트로 동기화되어 모든 client(host)가 같은 상태를 유지한다
	 * objectId는 네트워크에서 각 client(host) 소유의 object를 구분하기 위한 고유 식별자
	 * objectId는 {client id}_{counter} 형식이고 고유하다
	 */
	HashSet<string> _objectIdPool = new HashSet<string>();
	uint _objectIdCounter;

	public override void OnNetworkSpawn()
	{
		if (_poolConfig.Count > 0)
		{
			foreach (var config in _poolConfig)
			{
				var prefabId = config.PrefabId;
				var pool = new UnityEngine.Pool.ObjectPool<IDynamicPooledObject>(
				createFunc: () =>
				{
					var obj = GameObject.Instantiate(config.Prefab);
					var pdo = obj.GetComponent<IDynamicPooledObject>();

#if UNITY_EDITOR
					if (pdo == null)
					{
						throw new NullReferenceException("No IDynamicPooledObject component founded");
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
					Destroy(instance.GO);
				},
				collectionCheck: true,
				defaultCapacity: config.Capacity);

				_configs[prefabId] = config;
				_pools[prefabId] = pool;
			}
		}
	}

	public void PrintPoolStatus()
	{
		//lock (_activatedObjects)
		{
			GLogger.Log($"Activated count: {_activatedObjects.Count}");
		}

		//lock (_objectIdPool)
		{
			GLogger.Log($"object id pool count: {_objectIdPool.Count}");
		}
	}

	/*
	 * projectile 생성용
	 */
	public void CreateProjectile(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in ProjectileRpcParameter prp)
	{
		string objectId;
		//lock (_objectIdPool)
		{
			objectId = $"{NetworkManager.LocalClientId}_{_objectIdCounter}";
			//GLogger.Log($"{objectId}");
			//_objectIdPool.Add(objectId);
			_objectIdCounter++;
		}

		CreateProjectileEveryoneRpc(
			prefabId,
			objectId,
			position,
			rotation,
			prp);
	}

	[Rpc(SendTo.Everyone)]
	void CreateProjectileEveryoneRpc(
		FixedString32Bytes prefabId,
		FixedString32Bytes objectId,
		Vector2 position,
		Quaternion rotation,
		ProjectileRpcParameter prp,
		RpcParams rpcParams = default)
	{
		var pi = prefabId.ToString();
		var oi = objectId.ToString();

		if (!_pools.TryGetValue(pi, out var pool))
		{
			Debug.LogWarning($"PooledDynamicSpawner.CreateObject unknown prefab {prefabId}");
			return;
		}

		IDynamicPooledObject dpo;
		//lock (pool)
		{
			dpo = pool.Get();
		}

		dpo.GO.SetActive(true);
		dpo.IsIllusion = rpcParams.Receive.SenderClientId != NetworkManager.LocalClientId;
		dpo.PrefabId = pi;
		dpo.ObjectId = oi;
		dpo.OwnerClientId = rpcParams.Receive.SenderClientId;
		dpo.IPDS = this;
		dpo.SetTransform(position, rotation);

		var ps = dpo.GO.GetComponent<IProjectileSetting>();
		ps.SetProjectileParameter(prp);

		//lock (_activatedObjects)
		{
			_activatedObjects[oi] = dpo;
		}

		//lock (_objectIdPool)
		{
			_objectIdPool.Add(oi);
		}
	}

	/*
	 * 이펙트 생성용
	 */
	public void CreateEffect(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in EffectRpcParameter erp,
		bool reliable = true)
	{
		if (reliable)
		{
			CreateEffectEveryoneReliableRpc(prefabId, position, rotation, erp);
		}
		else
		{
			CreateEffectEveryoneUnreliableRpc(prefabId, position, rotation, erp);
		}
	}

	/*
	 * 신뢰성 전송한다
	 */
	[Rpc(SendTo.Everyone, Delivery = RpcDelivery.Unreliable)]
	void CreateEffectEveryoneReliableRpc(
		FixedString32Bytes prefabId,
		Vector2 position,
		Quaternion rotation,
		EffectRpcParameter erp,
		RpcParams rpcParams = default)
	{
		CreateEffectImplementation(prefabId, position, rotation, erp, rpcParams);
	}

	/*
	 * 비신뢰성 전송한다
	 */
	[Rpc(SendTo.Everyone, Delivery = RpcDelivery.Unreliable)]
	void CreateEffectEveryoneUnreliableRpc(
		FixedString32Bytes prefabId,
		Vector2 position,
		Quaternion rotation,
		EffectRpcParameter erp,
		RpcParams rpcParams = default)
	{
		CreateEffectImplementation(prefabId, position, rotation, erp, rpcParams);
	}

	void CreateEffectImplementation(
		FixedString32Bytes prefabId,
		Vector2 position,
		Quaternion rotation,
		in EffectRpcParameter erp,
		RpcParams rpcParams)
	{
		var pi = prefabId.ToString();

		if (!_pools.TryGetValue(pi, out var pool))
		{
			Debug.LogWarning($"PooledDynamicSpawner.CreateObject unknown prefab{prefabId}");
			return;
		}

		IDynamicPooledObject dpo;
		//lock (pool)
		{
			dpo = pool.Get();
		}

		dpo.GO.SetActive(true);
		dpo.PrefabId = pi;
		dpo.ObjectId = "none";
		dpo.OwnerClientId = rpcParams.Receive.SenderClientId;
		dpo.IPDS = this;
		dpo.SetTransform(position, rotation);

		var es = dpo.GO.GetComponent<IEffectSetting>();
		es.SetEffectParameter(erp);
	}

	public void ReleaseObject(IDynamicPooledObject dpo)
	{
		ReleaseObjectRpc(dpo.PrefabId, dpo.ObjectId);
	}

	[Rpc(SendTo.Everyone)]
	void ReleaseObjectRpc(FixedString32Bytes prefabId, FixedString32Bytes objectId, RpcParams rpcParam = default)
	{
		var pi = prefabId.ToString();
		var oi = objectId.ToString();

		IDynamicPooledObject dpo;
		//lock (_activatedObjects)
		{
			if (_activatedObjects.TryGetValue(oi, out var obj))
			{
				dpo = obj;
			}
			else
			{
				Debug.LogWarning($"PooledDynamicSpawner.ReleaseObjectImplement no activated object found({oi})");
				return;
			}

			_activatedObjects.Remove(oi);
		}

		//lock (_pools)
		{
			if (_pools.TryGetValue(pi, out var pool))
			{
				try
				{
					pool.Release(dpo);
				}
				catch (InvalidOperationException e)
				{
					GLogger.LogException(e);
				}
			}
			else
			{
				GLogger.LogWarning($"PooledDynamicSpawner.ReleaseObjectImplement unknown pool({prefabId})");
			}
		}

		//lock (_objectIdPool)
		{
			_objectIdPool.Remove(oi);
		}
	}

	/*
	 * effect object는 스스로 pool에 반납할 수 있게 한다
	 */
	public void ReleaseEffectObject(IDynamicPooledObject dpo)
	{
		//lock (_pools)
		{
			if (_pools.TryGetValue(dpo.PrefabId, out var pool))
			{
				try
				{
					pool.Release(dpo);
				}
				catch (InvalidOperationException e)
				{
					GLogger.LogException(e);
				}
			}
			else
			{
				GLogger.LogWarning($"PooledDynamicSpawner.ReleaseObjectImplement unknown pool({dpo.PrefabId})");
			}
		}
	}

	/*
	 * 가능하면 pool에 들어있는 object는 스스로 상태를 조절하여 release되도록 한다
	 */
	//public void RemovePlayerObjects(ulong clientId)
	//{
	//	RemovePlayerObjectsRpc(clientId);
	//}

	//[Rpc(SendTo.Everyone)]
	//void RemovePlayerObjectsRpc(ulong clientId)
	//{
	//	Task.Run(() =>
	//	{
	//		List<string> listMatched;
	//		Dictionary<string, IDynamicPooledObject> temp = new();
	//		lock (_activatedObjects)
	//		{
	//			var regex = new Regex(@$"{clientId}_");
	//			listMatched = _activatedObjects.Keys.Where(key => regex.IsMatch(key)).ToList();

	//			foreach (var key in listMatched)
	//			{
	//				temp[key] = _activatedObjects[key];
	//				_activatedObjects.Remove(key);
	//			}
	//		}

	//		lock(_pools)
	//		{
	//			foreach (var pair in temp)
	//			{
	//				_pools[pair.Key].Release(pair.Value);
	//			}
	//		}
	//	});
	//}
}