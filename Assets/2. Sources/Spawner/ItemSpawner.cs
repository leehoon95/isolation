using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

/*
 * in-scene placed 객체로 있어야 함
 * item prefab을 spawn
 */
public class ItemSpawner : NetworkBehaviour
{
	[SerializeField]
	GameObject _prefabToSpawn;

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

		/*
		 * 아이템 개수가 20개를 넘어가면 제일 오래된 item부터 despawn한다
		 */
		//if (_activedItems.Count > 20)
		//{
		//	var oldItem = _activedItems.Values.OrderBy(item => item.SpawnedTime).FirstOrDefault();
		//	oldItem.NO.Despawn();
		//}
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
		 * Despawn시 바로 Destroy메서드가 호출됨
		 * 이 메서드는 Rpc으로 메인 스레드에서 실행되고 Destroy 메서드 또한 그렇다
		 * C#의 재진입성(Reentrancy) 특성 때문에 foreach문 안에서 Despawn을 호출하면 바로 Destroy메서드를 실행하고 복귀한다
		 * Destroy메서드에 같은 컬렉션을 수정하는 코드가 있기 때문에 foreach문에서 InvalidOperationException 예외가 발생한다
		 * lock을 사용해도 같은 스레드가 소유했다면 차단하지 않고 통과하기 때문에 같은 예외가 발생한다
		 * 따라서 컬렉션의 item을 따로 복사한 뒤에 Despawn해야 한다
		 */
		var items = _activedItems.Values.ToList();

		foreach (var item in items)
		{
			//GLogger.Log($"Despawn {item}");
			item.NO.Despawn();
		}
	}

	void OnNetworkObjectDestroyed(NetworkObject networkObject)
	{
		_activedItems.Remove(networkObject.NetworkObjectId);
	}
}
