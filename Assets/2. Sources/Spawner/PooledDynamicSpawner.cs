using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

/*
 * prefab 용도에 따라 PrefabId으로 구분하고 해당하는 pool을 생성하기 위한 설정클래스
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
	 * inspector에서 설정한 pool config에 따라 pool 초기화
	 */
	Dictionary<string, PoolConfig> _configs = new();

	/*
	 * prefapId으로 원하는 prefab을 관리하는 pool을 검색
	 */
	Dictionary<string, ObjectPool<IDynamicPooledObject>> _pools = new();
	
	/*
	 * 활성화된 object를 object id를 키로 보관
	 */
	Dictionary<string, IDynamicPooledObject> _activatedObjects = new();

	/*
	 * 생성과 삭제는 동기화되어 모든 client(host)가 같은 상태를 유지
	 * object id는 네트워크에서 고유 식별자를 가짐
	 * object id는 {client id}_{counter} 형식이다
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
				var pool = new ObjectPool<IDynamicPooledObject>(
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


	public void CreateProjectile(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in ProjectileRpcParameter prp)
	{
		string objectId;
		objectId = $"{NetworkManager.LocalClientId}_{_objectIdCounter}";
		//GLogger.Log($"{objectId}");
		//_objectIdPool.Add(objectId);
		_objectIdCounter++;

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

		dpo = pool.Get();
		dpo.GO.SetActive(true);
		dpo.IsIllusion = rpcParams.Receive.SenderClientId != NetworkManager.LocalClientId;
		dpo.PrefabId = pi;
		dpo.ObjectId = oi;
		dpo.OwnerClientId = rpcParams.Receive.SenderClientId;
		dpo.IPDS = this;
		dpo.SetTransform(position, rotation);

		var ps = dpo.GO.GetComponent<IProjectileSetting>();
		ps.SetProjectileParameter(prp);

		_activatedObjects[oi] = dpo;
		_objectIdPool.Add(oi);
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

	public void CreateEffectLocal(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in EffectRpcParameter erp)
	{
		CreateEffectImplementation(
			prefabId, position, rotation, erp,
			new RpcParams() { 
				Receive = new RpcReceiveParams() { 
					SenderClientId = NetworkManager.LocalClientId } 
			});
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
		dpo = pool.Get();
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

		_objectIdPool.Remove(oi);
	}

	/*
	 * effect object는 스스로 pool에 반납할 수 있다
	 */
	public void ReleaseEffectObject(IDynamicPooledObject dpo)
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

	public void PrintPoolStatus()
	{
		GLogger.Log($"Activated count: {_activatedObjects.Count}");
		GLogger.Log($"object id pool count: {_objectIdPool.Count}");
	}
}