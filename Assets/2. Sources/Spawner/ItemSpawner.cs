using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

[Serializable]
public struct InSceneItemConfig
{
	public string ItemEffect;
	public Vector2 Position;
}

/*
 * in-scene placed 객체로 있어야 함
 */
public class ItemSpawner : NetworkBehaviour
{
	[SerializeField]
	GameObject _prefabToSpawn;
	[SerializeField]
	List<InSceneItemConfig> _inSceneConfigs;

	SpawnItemWithDataHandler _spawnHandler;
	Dictionary<ulong, IItemHandler> _activedItems;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_spawnHandler = new SpawnItemWithDataHandler(networkManager, _prefabToSpawn);
		if (IsHost)
		{
			_activedItems = new();
			_spawnHandler.NetworkObjectDestroyed += OnNetworkObjectDestroyed;
		}
	}

	[Rpc(SendTo.Server)]
	public void SpawnItemRpc(
		Vector2 spawnPosition,
		Quaternion rotation,
		ItemInstantiateData data,
		RpcParams rpcParam = default)
	{
		var item = _spawnHandler.InstantiateWithDataAndSpawn(
			spawnPosition, rotation, data);
		_activedItems[item.NO.NetworkObjectId] = item;
	}

	[Rpc(SendTo.Server)]
	public void DespawnAllItemsRpc()
	{
		if (_activedItems.Count == 0)
		{
			return;
		}

		/*
		 * _activedItems를 직접 순회하면서 Despawn하지 않는 이유
		 * _activedItems 순회 시작 -> Despawn호출 -> SpawnItemWithDataHandler.Destroy 호출 -> OnNetworkObjectDestroyed 호출 -> 순회중 _activedItems.Remove 메서드 호출 -> InvalidOperationException 예외 발생
		 * C# lock문은 재진입성(Reentrancy)을 허용하여 lock을 획득한 스레드가 다시 lock 블록에 진입하면 대기하지 않고 통과된다
		 * lock을 사용해도 같은 스레드가 소유했다면 차단하지 않고 통과하기 때문에 같은 예외가 발생한다
		 * 따라서 컬렉션의 item을 따로 복사한 뒤에 Despawn해야 한다
		 */
		var items = _activedItems.Values.ToList();

		foreach (var item in items)
		{
			item.NO.Despawn();
		}
	}

	void OnNetworkObjectDestroyed(NetworkObject networkObject)
	{
		_activedItems.Remove(networkObject.NetworkObjectId);
	}

	public void SpawnFieldItems()
	{
		if (IsHost)
		{
			ItemInstantiateData itemInitData = new();
			foreach (var config in _inSceneConfigs)
			{
				itemInitData.ItemEffect = config.ItemEffect;
				var item = _spawnHandler.InstantiateWithDataAndSpawn(
					config.Position, Quaternion.identity, itemInitData);
				_activedItems[item.NO.NetworkObjectId] = item;
			}
		}
	}
}
