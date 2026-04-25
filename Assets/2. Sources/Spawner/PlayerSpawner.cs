using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

/*
 * in-scene placed 객체로 있어야 함
 * player prefab을 spawn
 */
public class PlayerSpawner : NetworkBehaviour, IPlayerSpawner
{
	[SerializeField]
	GameObject _prefabPlayer;
	[SerializeField]
	GameObject _prefabDeadBody;
	[SerializeField]
	InputSystem _inputSystem;
	[SerializeField]
	PooledDynamicSpawner _pooledDynamicSpawner;

	SpawnPlayerWithDataHandler _playerSpawnHandler;
	SpawnPlayerDeadBodyWithDataHandler _playerDeadBodySpawnHandler;
	// 생존 player 리스트를 외부에 전달하기 위함(host전용)
	Dictionary<ulong, IPlayerHandler> _activedPlayer;
	Dictionary<ulong, IPlayerDeadBodyHandler> _activedDeadBody;
	IPlayerSpawnEventRaiserble _pser;

	public event UnityAction<IPlayerHandler> PlayerSpawned;
	public event UnityAction<IPlayerHandler> PlayerDespawned;

	protected override void OnNetworkPreSpawn(ref NetworkManager networkManager)
	{
		_playerSpawnHandler = new SpawnPlayerWithDataHandler(
			networkManager,
			_prefabPlayer,
			this,
			_pooledDynamicSpawner,
			_inputSystem);

		_playerDeadBodySpawnHandler = new SpawnPlayerDeadBodyWithDataHandler(
			networkManager,
			_prefabDeadBody,
			this);

		_activedPlayer = new();

		if (networkManager.IsHost)
		{
			
			_activedDeadBody = new();
			//_playerSpawnHandler.PlayerObjectInstantiated += OnPlayerObjectInstantiated;
			//_playerSpawnHandler.PlayerObjectDestroyed += OnPlayerObjectDestroyed;
		}
	}

	/*
	 * host는 handler 메서드를 사용해서 바로 스폰한다.
	 * client side에서는 handler를 통해 prefab override를 진행한다
	 */
	[Rpc(SendTo.Server)]
	public void SpawnPlayerRpc(
		ulong ownerId,
		Vector2 spawnPosition,
		Quaternion rotation,
		PlayerInstantiateData data,
		RpcParams rpcParam = default)
	{
		var pp = _playerSpawnHandler.InstantiateWithDataAndSpawn(
			ownerId,
			spawnPosition, 
			rotation, 
			data);

		_activedPlayer[pp.NO.NetworkObjectId] = pp;
	}

	[Rpc(SendTo.Server)]
	public void DespawnPlayerRpc(RpcParams rpcParam = default)
	{
		_playerSpawnHandler.DespawnPlayer(rpcParam.Receive.SenderClientId);
	}

	[Rpc(SendTo.Server)]
	public void DespawnAllPlayersRpc(RpcParams rpcParam = default)
	{
		_playerSpawnHandler.DesapwnAllPlayers();
	}

	//[Rpc(SendTo.Server)]
	//public void RevivePlayerRpc(
	//	ulong ownerId, 
	//	Vector2 spawnPosition, 
	//	Quaternion rotation,
	//	PlayerInstantiateData data,
	//	RpcParams rpcParam = default)
	//{
	//	var pp = _playerSpawnHandler.InstantiateWithDataAndSpawn(
	//		ownerId,
	//		spawnPosition, rotation, data);

	//	_activedPlayer[pp.NO.NetworkObjectId] = pp;
	//}

	[Rpc(SendTo.Server)]
	public void SpawnPlayerDeadBodyRpc(
		Vector2 spawnPosition, 
		Quaternion rotation,
		FixedString32Bytes nickname,
		Color personalColor,
		RpcParams rpcParam = default)
	{
		var db = _playerDeadBodySpawnHandler.InstantiateWithDataAndSpawn(
			rpcParam.Receive.SenderClientId,
			spawnPosition, 
			rotation,
			new PlayerDeadBodyInstantiateData { 
				Nickname = nickname,
				PersonalColor = personalColor,
			});

		_activedDeadBody[db.NO.NetworkObjectId] = db;
	}

	//void OnPlayerObjectDestroyed(NetworkObject networkObject)
	//{
	//	_activedPlayer.Remove(networkObject.NetworkObjectId);
	//}

	//void DeadboyObjectDestroyed(NetworkObject networkObject)
	//{
	//	_activedDeadBody.Remove(networkObject.NetworkObjectId);
	//}

	public List<IPlayerHandler> GetPlayers()
	{
		return _activedPlayer.Values.ToList();
	}
	public IPlayerHandler GetPlayer(ulong id)
	{
		
		if (_activedPlayer.TryGetValue(id, out var player))
		{
			return player;
		}
		else
		{
			return null;
		}
	}

	public void NotifyPlayerSpawned(IPlayerHandler ph)
	{
		GLogger.Log($"NotifyPlayerSpawned count: {_activedPlayer.Count}");
		PlayerSpawned?.Invoke(ph);
		_activedPlayer[ph.SpawnClientId] = ph;
	}

	public void NotifyPlayerDespawned(IPlayerHandler ph)
	{
		PlayerDespawned?.Invoke(ph);
		_activedPlayer.Remove(ph.SpawnClientId);
	}
}
