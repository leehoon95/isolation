using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public struct PlayerInstantiateData : INetworkSerializable
{
	public FixedString64Bytes Nickname;
	public Color PersonalColor;
	// 필요시 추가하고, NetworkSerialize에도 추가할 것

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Nickname);
		serializer.SerializeValue(ref PersonalColor);
	}
}

public class SpawnPlayerWithDataHandler : NetworkPrefabInstanceHandlerWithData<PlayerInstantiateData>
{
	GameObject _prefabToSpawn;
	NetworkManager _networkManager;
	Dictionary<ulong, IPlayerHandler> _instances = new();
	IPlayerSpawnObserver _playerSpawnObserver;
	IPooledDynamicSpawner _pooledDynamicSpawner;
	InputSystem _inputSystem;

	public event Action<NetworkObject> PlayerObjectDestroyed;

	public SpawnPlayerWithDataHandler(
		NetworkManager networkManager, 
		GameObject perfab,
		IPlayerSpawnObserver playerSpawnObserver,
		IPooledDynamicSpawner pooledDynamicSpawner,
		InputSystem inputSystem)
	{
		_prefabToSpawn = perfab;
		_networkManager = networkManager;
		_playerSpawnObserver = playerSpawnObserver;
		_pooledDynamicSpawner = pooledDynamicSpawner;
		_inputSystem = inputSystem;

		_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);
	}

	public IPlayerHandler InstantiateWithDataAndSpawn(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		if (!_networkManager.IsServer)
		{
			return null;
		}

		var instance = GetPrefabInstance(ownerClientId, position, rotation, instantiationData);
		_networkManager.PrefabHandler.SetInstantiationData(instance.NO, instantiationData);
		instance.SpawnObserver = _playerSpawnObserver;
		instance.IPDS = _pooledDynamicSpawner;
		instance.InputSystem = _inputSystem;
		instance.NO.SpawnWithOwnership(ownerClientId, true);
		
		return instance;
	}

	public void InactiveAndDespawn(ulong ownerClientId)
	{
		if (!_networkManager.IsServer)
		{
			return;
		}

		if (_instances.ContainsKey(ownerClientId))
		{
			var instance = _instances[ownerClientId];
			instance.NO.Despawn();
		}
	}

	IPlayerHandler GetPrefabInstance(ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		IPlayerHandler instance = null;
		if (_instances.ContainsKey(ownerClientId))
		{
			instance = _instances[ownerClientId];
			instance.GO.SetActive(true);
		}
		else
		{
			instance = UnityEngine.Object.Instantiate(_prefabToSpawn).GetComponent<PointmanPlayer>();
			_instances[ownerClientId] = instance;
		}

		instance.GO.transform.SetPositionAndRotation(position, rotation);
		instance.PersonalColor = instantiationData.PersonalColor;

		return instance;
	}

	// client에서만 호출된다.
	public override NetworkObject Instantiate(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(ownerClientId, position, rotation, instantiationData);
		instance.SpawnObserver = _playerSpawnObserver;
		instance.IPDS = _pooledDynamicSpawner;
		instance.InputSystem = _inputSystem;

		return instance.NO;
	}

	// host, client 모두 호출된다
	public override void Destroy(NetworkObject networkObject)
	{
		PlayerObjectDestroyed?.Invoke(networkObject);
		networkObject.gameObject.SetActive(false);
	}
}