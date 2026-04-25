using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public struct PlayerDeadBodyInstantiateData : INetworkSerializable
{
	public FixedString64Bytes Nickname;
	public Color PersonalColor;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Nickname);
		serializer.SerializeValue(ref PersonalColor);
	}
}

public class SpawnPlayerDeadBodyWithDataHandler : NetworkPrefabInstanceHandlerWithData<PlayerDeadBodyInstantiateData>
{
	GameObject _prefabToSpawn;
	NetworkManager _networkManager;
	Dictionary<ulong, IPlayerDeadBodyHandler> _instances = new();
	IPlayerSpawner _playerSpawner;

	public SpawnPlayerDeadBodyWithDataHandler(
		NetworkManager networkManager,
		GameObject perfab,
		IPlayerSpawner playSpawner)
	{
		_prefabToSpawn = perfab;
		_networkManager = networkManager;
		_playerSpawner = playSpawner;
		_networkManager.PrefabHandler.AddHandler(_prefabToSpawn, this);
	}

	public IPlayerDeadBodyHandler InstantiateWithDataAndSpawn(
		ulong ownerClientId,
		Vector3 position, 
		Quaternion rotation,
		PlayerDeadBodyInstantiateData instantiationData)
	{
		var instance = GetPrefabInstance(ownerClientId, position, rotation);
		_networkManager.PrefabHandler.SetInstantiationData(instance.NO, instantiationData);
		instance.GO.transform.SetPositionAndRotation(position, rotation);
		instance.Nickname = instantiationData.Nickname.ToString();
		instance.PersonalColor = instantiationData.PersonalColor;
		instance.Spawner = _playerSpawner;
		instance.ClientIdForRevive = ownerClientId;
		instance.NO.Spawn(true);

		return instance;
	}

	IPlayerDeadBodyHandler GetPrefabInstance(
		ulong ownerClientId,
		Vector3 position, 
		Quaternion rotation)
	{
		IPlayerDeadBodyHandler instance = null;
		if (_instances.TryGetValue(ownerClientId, out var obj))
		{
			instance = obj;
			instance.GO.SetActive(true);
		}
		else
		{
			instance = Object.Instantiate(_prefabToSpawn).GetComponent<IPlayerDeadBodyHandler>();
			_instances[ownerClientId] = instance;
		}

		return instance;
	}

	public override NetworkObject Instantiate(
		ulong ownerClientId, 
		Vector3 position, 
		Quaternion rotation, 
		PlayerDeadBodyInstantiateData instantiationData)
	{
		GLogger.Log($"Instantiate deadbody {position}");
		var instance = GetPrefabInstance(
			ownerClientId,
			position,
			rotation);
		instance.GO.transform.SetPositionAndRotation(position, rotation);
		instance.Nickname = instantiationData.Nickname.ToString();
		instance.PersonalColor = instantiationData.PersonalColor;
		instance.Spawner = _playerSpawner;

		return instance.NO;
	}

	public override void Destroy(NetworkObject networkObject)
	{
		networkObject.gameObject.SetActive(false);
	}
}
