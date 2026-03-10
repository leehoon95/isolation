using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public struct EnemyInstantiateData : INetworkSerializable
{
	public string PrefabId;
	public int MaxHealthPoint;
	public float Speed;
	public float KnockbackResistance;
	public float StoppingPowerResistance;
	public int Defense;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref PrefabId);
		serializer.SerializeValue(ref MaxHealthPoint);
		serializer.SerializeValue(ref Speed);
		serializer.SerializeValue(ref KnockbackResistance);
		serializer.SerializeValue(ref StoppingPowerResistance);
		serializer.SerializeValue(ref Defense);
	}
}

public class EnemyPrefabWithDataHandler : NetworkPrefabInstanceHandlerWithData<EnemyInstantiateData>
{
	GameObject _prefabToSpawn;
	NetworkManager _networkManager;
	IEnemySpawner _spawner;
	IPooledDynamicSpawner _ipds;
	
	Dictionary<string, PoolConfig> _poolConfig = new();
	Dictionary<string, ObjectPool<IEnemyHandler>> _pools = new();
	public event Action<NetworkObject> NetworkObjectDestroyed;

	public EnemyPrefabWithDataHandler(
		NetworkManager networkManager, 
		GameObject prefabToSpawn, 
		List<PoolConfig> poolConfigs,
		IEnemySpawner spawner,
		IPooledDynamicSpawner pds)
	{
		_networkManager = networkManager;
		_prefabToSpawn = prefabToSpawn;
		_spawner = spawner;
		_ipds = pds;
		//_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);

		foreach (var config in poolConfigs)
		{	
			var pool = new ObjectPool<IEnemyHandler>(
			createFunc: () =>
			{
				var obj = UnityEngine.Object.Instantiate(config.Prefab);
				var ih = obj.GetComponent<IEnemyHandler>();
				ih.PrefabId = config.PrefabId;

#if UNITY_EDITOR
				if (ih == null)
				{
					throw new NullReferenceException("No IEnemyHandler interface founded");
				}
#endif
				return ih;
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
					UnityEngine.Object.Destroy(instance.GO);
				},
				true,
				config.Capacity
			);

			_poolConfig[config.PrefabId] = config;
			_pools[config.PrefabId] = pool;
			_networkManager.PrefabHandler.AddHandler(config.Prefab, this);
		}
	}

	public IEnemyHandler InstantiateWithDataAndSpawn(
		Vector3 position, Quaternion rotation,
		in EnemyInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(position, rotation, instantiationData);
		_networkManager.PrefabHandler.SetInstantiationData(instance.GO, instantiationData);
		instance.SetData(instantiationData);
		instance.NO.Spawn(true);

		return instance;
	}

	IEnemyHandler GetPrefabInstance(Vector2 position, Quaternion rotation,
		in EnemyInstantiateData enemyInstantiateData)
	{
		var pool = _pools[enemyInstantiateData.PrefabId];
		var obj = pool.Get();
		obj.GO.SetActive(true);
		obj.GO.transform.SetPositionAndRotation(position, rotation);
		obj.Spawner = _spawner;
		obj.IPDS = _ipds;

		return obj;
	}

	public override NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation, EnemyInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(position, rotation, instantiationData);
		return instance.NO;
	}

	public override void Destroy(NetworkObject networkObject)
	{
		NetworkObjectDestroyed?.Invoke(networkObject);

		var ih = networkObject.GetComponent<IEnemyHandler>();
		_pools[ih.PrefabId].Release(ih);

		//_pool.Release(networkObject.GetComponent<IEnemyHandler>());
	}
}
