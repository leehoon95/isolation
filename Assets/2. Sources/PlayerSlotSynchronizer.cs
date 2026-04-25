using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/*
 * 입장한 client 순으로 slot을 배정한다
 */
public struct SlotSyncData : IEquatable<SlotSyncData>, INetworkSerializable
{
	// string 타입은 허용하지 않는다. string은 C# immutable 타입
	public FixedString64Bytes Nickname;
	public Color PersonalColor;
	public bool Ready;

	public bool Equals(SlotSyncData other)
	{
		return Nickname == other.Nickname && PersonalColor == other.PersonalColor && Ready == other.Ready;
	}

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Nickname);
		serializer.SerializeValue(ref PersonalColor);
		serializer.SerializeValue(ref Ready);
	}
}

/*
 * 메인스레드에서만 접근하고 Host만 내부 데이터를 변경할 것
 */
public class PlayerSlotSynchronizer : NetworkBehaviour
{
	// index->clientId
	//NetworkList<ulong> _clientSlotOrder = new();
	NetworkList<SlotSyncData> _slotSyncDatas = new();
	NetworkList<ulong> _clientIds = new();

	public event Action<int> OnSlotDataChanged;

	/*
	 * clienId-sync data index 캐시
	 */
	Dictionary<ulong, int> _index = new();

	// NetworkVariable 필드의 동기화가 OnNetworkSpawn 호출보다 먼저 실행된다
	public override void OnNetworkSpawn()
	{
		_slotSyncDatas.OnListChanged += (e) =>
		{
			//GLogger.LogWarning($"OnListChanged {e.Index} {e.PreviousValue.Ready} {e.Value.Ready}");
			OnSlotDataChanged?.Invoke(_slotSyncDatas.Count);
		};
	}

	[Rpc(SendTo.Server)]
	public void AddClientRpc(ulong clientId, FixedString64Bytes nickname, Color color)
	{
		if (!NetworkManager.Singleton.IsHost)
		{
			GLogger.LogWarning("PlayerSlotSynchronizer.AddClient You are not host");
			return;
		}
		_slotSyncDatas.Add(new SlotSyncData() { 
			Nickname = nickname,
			PersonalColor = color,
			Ready = false
		});
		_clientIds.Add(clientId);
	}

	[Rpc(SendTo.Server)]
	public void RemoveClientRpc(ulong clientID) 
	{
		if (!NetworkManager.Singleton.IsHost)
		{
			GLogger.LogWarning("PlayerSlotSynchronizer.RemoveClient You are not host");
			return;
		}

		var index = _clientIds.IndexOf(clientID);
		_slotSyncDatas.RemoveAt(index);
		_clientIds.RemoveAt(index);
	}

	[Rpc(SendTo.Server)]
	public void ReadyClientRpc(ulong clientId, bool ready)
	{
		if (!NetworkManager.Singleton.IsHost)
		{
			GLogger.LogWarning("PlayerSlotSynchronizer.ReadyClient You are not host");
			return;
		}
		GLogger.Log($"ReadyClientRpc {ready}");

		var index = _clientIds.IndexOf(clientId);
		var data = _slotSyncDatas[index];
		data.Ready = ready;
		_slotSyncDatas[index] = data;
	}

	public int GetSlotDataCount() => _slotSyncDatas.Count;
	public SlotSyncData GetSlotData(int index) => _slotSyncDatas[index];
}
