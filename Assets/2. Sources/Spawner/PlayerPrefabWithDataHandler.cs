using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

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
	Dictionary<ulong, NetworkObject> _instances = new();

	public SpawnPlayerWithDataHandler(NetworkManager networkManager, GameObject perfab)
	{
		_prefabToSpawn = perfab;
		_networkManager = networkManager;

		_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);
	}

	public NetworkObject InstantiateWithDataAndSpawn(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		if (!_networkManager.IsServer)
		{
			return null;
		}

		var instance = GetPrefabInstance(ownerClientId, position, rotation, instantiationData);
		_networkManager.PrefabHandler.SetInstantiationData(instance, instantiationData);
		instance.SpawnWithOwnership(ownerClientId, true);

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
			if (instance.IsSpawned)
			{
				instance.Despawn();
			}
		}
	}

	NetworkObject GetPrefabInstance(ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		NetworkObject instance = null;
		if (_instances.ContainsKey(ownerClientId))
		{
			instance = _instances[ownerClientId];
			instance.gameObject.SetActive(true);
		}
		else
		{
			instance = Object.Instantiate(_prefabToSpawn).GetComponent<NetworkObject>();
			_instances[ownerClientId] = instance;
		}

		instance.transform.SetPositionAndRotation(position, rotation);
		var ps = instance.GetComponent<IPlayerSetting>();
		ps.PersonalColor = instantiationData.PersonalColor;

		return instance;
	}

	// client에서만 호출된다.
	public override NetworkObject Instantiate(
		ulong ownerClientId,
		Vector3 position, Quaternion rotation,
		PlayerInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(ownerClientId, position, rotation, instantiationData);

		return instance;
	}

	// host, client 모두 호출된다
	public override void Destroy(NetworkObject networkObject)
	{
		networkObject.gameObject.SetActive(false);
	}
}