using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

public struct ItemInstantiateData : INetworkSerializable
{
	public FixedString32Bytes ItemEffect;
	public int ItemType;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref ItemEffect);
		serializer.SerializeValue(ref ItemType);
	}
}

public class SpawnItemWithDataHandler : NetworkPrefabInstanceHandlerWithData<ItemInstantiateData>
{
	GameObject _prefabToSpawn;
	NetworkManager _networkManager;
	ObjectPool<IItemHandler> _pool;
	public event Action<NetworkObject> NetworkObjectDestroyed;

	public SpawnItemWithDataHandler(NetworkManager networkManager, GameObject prefab)
	{
		_prefabToSpawn = prefab; 
		_networkManager = networkManager;
		_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);
		_pool = new ObjectPool<IItemHandler>(
			createFunc: () =>
			{
				var obj = UnityEngine.Object.Instantiate(_prefabToSpawn);
				var ih = obj.GetComponent<IItemHandler>();
#if UNITY_EDITOR
				if (ih == null)
				{
					throw new NullReferenceException("No IItemHandler component founded");
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
			true
		);
	}

	public IItemHandler InstantiateWithDataAndSpawn(
		Vector3 position, Quaternion rotation,
		in ItemInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(position, rotation, instantiationData);
		_networkManager.PrefabHandler.SetInstantiationData(instance.GO, instantiationData);
		instance.NO.Spawn(true);

		return instance;
	}

	IItemHandler GetPrefabInstance(
		Vector2 position, Quaternion rotation, 
		in ItemInstantiateData itemInstantiateData)
	{
		var obj = _pool.Get();
		obj.GO.SetActive(true);
		obj.GO.transform.SetPositionAndRotation(position, rotation);
		var ih = obj.GO.GetComponent<IItemHandler>();
		ih.ItemType = (ItemType)itemInstantiateData.ItemType;
		ih.ItemEffect = itemInstantiateData.ItemEffect.ToString();
		ih.RefreshItemShape();

		return obj;
	}

	public override NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation, ItemInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(position, rotation, instantiationData);
		
		return instance.NO;
	}

	public override void Destroy(NetworkObject networkObject)
	{
		NetworkObjectDestroyed?.Invoke(networkObject);
		_pool.Release(networkObject.GetComponent<IItemHandler>());
	}
}
