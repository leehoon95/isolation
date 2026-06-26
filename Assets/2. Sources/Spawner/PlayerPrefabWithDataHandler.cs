using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerInstantiateData : INetworkSerializable
{
	public ulong OwnerClientId;
	public Vector3 Position;
	public FixedString64Bytes Nickname;
	public Color PersonalColor;
	public bool AutomaticMotion;
	// 필요시 추가하고, NetworkSerialize에도 추가할 것

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref OwnerClientId);
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref Nickname);
		serializer.SerializeValue(ref PersonalColor);
		serializer.SerializeValue(ref AutomaticMotion);
	}
}

public class SpawnPlayerWithDataHandler : NetworkPrefabInstanceHandlerWithData<PlayerInstantiateData>
{
	GameObject _prefabToSpawn;
	NetworkManager _networkManager;
	Dictionary<ulong, IPlayerHandler> _instances = new();
	IPlayerSpawner _playerSpawner;
	IPooledDynamicSpawner _pooledDynamicSpawner;
	InputSystem _inputSystem;

	public event Action<NetworkObject> PlayerObjectDestroyed;

	public SpawnPlayerWithDataHandler(
		NetworkManager networkManager, 
		GameObject perfab,
		IPlayerSpawner playSpawner,
		IPooledDynamicSpawner pooledDynamicSpawner,
		InputSystem inputSystem)
	{
		_prefabToSpawn = perfab;
		_networkManager = networkManager;
		_playerSpawner = playSpawner;
		_pooledDynamicSpawner = pooledDynamicSpawner;
		_inputSystem = inputSystem;

		_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);
	}

	public IPlayerHandler InstantiateWithDataAndSpawn(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		//GLogger.Log($"Spawn Player {ownerClientId}");
		var instance = GetPrefabInstance(ownerClientId);
		_networkManager.PrefabHandler.SetInstantiationData(instance.NO, instantiationData);
		instance.GO.transform.position = position;
		instance.PersonalColor = instantiationData.PersonalColor;
		instance.Nickname = instantiationData.Nickname.ToString();
		instance.Spawner = _playerSpawner;
		instance.IPDS = _pooledDynamicSpawner;
		instance.InputSystem = _inputSystem;
		instance.SpawnClientId = ownerClientId;
		instance.AutomaticMotion = instantiationData.AutomaticMotion;

		/*
		 * SpawnWithOwnership 메서드는 스폰시 위치 지정이 되지 않는다
		 */
		//instance.NO.SpawnWithOwnership(ownerClientId, true);
		instance.NO.Spawn(true);
		instance.NO.ChangeOwnership(ownerClientId);

		return instance;
	}

	public void DespawnPlayer(ulong ownerClientId)
	{
		if (_instances.TryGetValue(ownerClientId, out var ph))
		{
			ph.NO.Despawn();
		}
	}

	public void DesapwnAllPlayers()
	{
		foreach (var p in _instances.Values)
		{
			p.NO.Despawn();
		}
	}

	IPlayerHandler GetPrefabInstance(ulong ownerClientId)
	{
		IPlayerHandler instance = null;
		if (_instances.TryGetValue(ownerClientId, out var obj))
		{
			instance = obj;
			instance.GO.SetActive(true);
		}
		else
		{
			instance = UnityEngine.Object.Instantiate(_prefabToSpawn).GetComponent<IPlayerHandler>();
			_instances[ownerClientId] = instance;
		}

		return instance;
	}

	// client에서만 호출된다.
	public override NetworkObject Instantiate(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(instantiationData.OwnerClientId);

		instance.PersonalColor = instantiationData.PersonalColor;
		instance.Nickname = instantiationData.Nickname.ToString();
		instance.Spawner = _playerSpawner;
		instance.IPDS = _pooledDynamicSpawner;
		instance.InputSystem = _inputSystem;
		instance.SpawnClientId = instantiationData.OwnerClientId;
		instance.AutomaticMotion = instantiationData.AutomaticMotion;

		return instance.NO;
	}

	// host, client 모두 호출된다
	public override void Destroy(NetworkObject networkObject)
	{
		PlayerObjectDestroyed?.Invoke(networkObject);
		networkObject.gameObject.SetActive(false);
	}
}