using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : NetworkBehaviour, IEnemySpawner
{
	[SerializeField]
	GameObject _prefabToSpawn;
	[SerializeField]
	PooledDynamicSpawner _pds;
	[SerializeField]
	PlayerSpawner _playerSpawner;
	[SerializeField]
	ItemSpawner _itemSpawner;
	[SerializeField]
	float _itemSpawnRate;
	[SerializeField]
	List<PoolConfig> _poolConfig;

	public event UnityAction<string> EnemySpawned;
	public event UnityAction<string, Vector2> EnemyDespawned;

	EnemyPrefabWithDataHandler _spawnHandler;
	Dictionary<ulong, IEnemyHandler> _activedEnemys;
	string[] _weaponNames = new[] {
		"bolt",
		"missile",
		"laser",
		"burst",
		"shield",
		"heal",
		"bomb",
	};

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_spawnHandler = new EnemyPrefabWithDataHandler(
			networkManager, 
			_prefabToSpawn, 
			_poolConfig,
			this,
			_pds);
		
		if (networkManager.IsHost)
		{
			_activedEnemys = new();
			_spawnHandler.NetworkObjectDestroyed += OnNetworkObjectDestroyed;
		}
	}

	[Rpc(SendTo.Server)]
	public void SpawnEnemyRpc(
		Vector2 spawnPosition,
		Quaternion rotation,
		EnemyInstantiateData data,
		RpcParams rpcParam = default)
	{
		var item = _spawnHandler.InstantiateWithDataAndSpawn(
			spawnPosition, rotation, data);
		_activedEnemys[item.NO.NetworkObjectId] = item;
	}

	[Rpc(SendTo.Server)]
	public void DespawnAllEnemysRpc()
	{
		if (_activedEnemys.Count == 0)
		{
			return;
		}

		EnemySpawned = null;
		EnemyDespawned = null;

		var items = _activedEnemys.Values.ToList();

		foreach (var item in items)
		{
			//GLogger.Log($"Despawn {item}");
			if (item.NO.IsSpawned)
			{
				item.NO.Despawn();
			}
		}

		_activedEnemys.Clear();
	}

	void OnNetworkObjectDestroyed(NetworkObject networkObject)
	{
		_activedEnemys.Remove(networkObject.NetworkObjectId);
	}

	public List<IEnemyHandler> GetEnemys()
	{
		return _activedEnemys.Values.ToList();
	}

	public void NotifyEnemySpawned(IEnemyHandler ph)
	{
		EnemySpawned?.Invoke(ph.PrefabId);
	}

	public void NotifyEnemyDespawned(IEnemyHandler ph)
	{
		EnemyDespawned?.Invoke(ph.PrefabId, ph.GO.transform.position);
		
		//if (Random.Range(0f, 100f) < _itemSpawnRate)
		//{
		//	_itemSpawner.SpawnItemRpc(
		//		ph.GO.transform.position,
		//		Quaternion.identity,
		//		new ItemInstantiateData()
		//		{
		//			ItemEffect = _weaponNames[UnityEngine.Random.Range(0, 7)]
		//		});
		//}
	}
}
