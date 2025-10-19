using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UGSLobbyManager : MonoBehaviour
{
	public static string PlayerName { get; set; }
	public static string PlayerLevel { get; set; }
	public static string HeartBeatTargetId { get; set; }
	public static Action<ILobbyChanges> OnLobbyChanged;
	public static Action OnKickedFromLobby;
	public static Action<LobbyEventConnectionState> OnLobbyEventConnectionStateChanged;


	static Player GetPlayer()
	{
		return new Player
		{
			Data = new Dictionary<string, PlayerDataObject>
			{
				{ "PlayerName",
					new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerName)},
				{ "PlayerLevel",
					new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerLevel)}
			},
			Profile = new PlayerProfile(PlayerName)
		};
	}

	/*
	 * 
	 * 
	 * 
	 */
	static void InvokeOnLobbyChanged(ILobbyChanges changes) => OnLobbyChanged?.Invoke(changes);
	static void InvokeOnKickedFromLobby() => OnKickedFromLobby?.Invoke();
	static void InvokeOnLobbyEventConnectionStateChanged(LobbyEventConnectionState changes)
		=> OnLobbyEventConnectionStateChanged?.Invoke(changes);
	

	public static async Awaitable<(bool, Lobby)> CreateLobby(
		string lobbyName, 
		int maxPlayers,
		string relayJoinCode,
		string password = "",
		bool inPrivate = false)
	{
		/*
		 * Player: 로비 만드는 player 정보
		 * IsPrivate: 로비를 공개한 것인가. 비공개면 LobbyCode를 이용해서 다른 사용자가 로비에 join 가능.
		 * Data: 로비에 적용하는 커스텀 게임 속성(map name, game type etc.)
		 */
		var createOptions = new CreateLobbyOptions
		{
			Player = GetPlayer(),
			IsPrivate = inPrivate,
			Password = password,
			Data = new Dictionary<string, DataObject>
			{
				{ "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Default")},
				{ "GamePlaying", new DataObject(DataObject.VisibilityOptions.Public, "StandBy")},
				{ "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode)}
			}
		};

		try
		{
			var lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createOptions);

			var callbacks = new LobbyEventCallbacks();
			callbacks.LobbyChanged += InvokeOnLobbyChanged;
			callbacks.KickedFromLobby += InvokeOnKickedFromLobby;
			callbacks.LobbyEventConnectionStateChanged += InvokeOnLobbyEventConnectionStateChanged;

			await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);

#if UNITY_EDITOR
			Debug.Log("Created Lobby Info\n" +
				$"id: {lobby.Id}\n" +
				$"private: {lobby.IsPrivate}\n" +
				$"max players: {lobby.MaxPlayers}\n" +
				$"created: {lobby.Created}"
				);
#endif

			return (true, lobby);
		}
		catch (ArgumentNullException e)
		{
			// Thrown when lobbyName is null or only contains whitespaces.
			Debug.LogError($"UGSLobbyManager.CreateLobby ArgumentNullException. message: {e.Message}");
			return (false, null);
		}
		catch (InvalidOperationException e)
		{
			// Thrown when maxPlayers is less than one.
			Debug.LogError($"UGSLobbyManager.CreateLobby InvalidOperationException. message: {e.Message}");
			return (false, null);
		}
		catch (LobbyServiceException e)
		{
			// Thrown when the lobby service returns an error.
			Debug.LogError($"UGSLobbyManager.CreateLobby LobbyServiceException. message: {e.Message}");
			return (false, null);
		}
	}

	public async Awaitable MaintainLobbyAlive(Lobby lobby)
	{
		if (lobby == null)
		{
			return;
		}

		/* 
		 * 기본 lobby 활성 주기 30초
		 * 주기적 하트비트 필요
		 * 사용하지 않는 lobby는 삭제해야 함
		 */
		await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
	}

	public static bool IsLobbyHost(Lobby lobby)
		=> lobby != null && (lobby.HostId == AuthenticationService.Instance.PlayerId);

	public static async Awaitable<List<Lobby>> GetLobbyList(bool isAvailableSlot = false, bool isPrivate = false)
	{
		try
		{
			/*
			 * Count: 결과 개수(1-100, default: 10)
			 * 
			 */
			var options = new QueryLobbiesOptions
			{
				Count = 100,
				Filters = new List<QueryFilter>
				{
					new QueryFilter(
						field: QueryFilter.FieldOptions.AvailableSlots,
						op: QueryFilter.OpOptions.GE,
						value: isAvailableSlot ? "1" : "0"),
					new QueryFilter(
						field: QueryFilter.FieldOptions.Created,
						op: QueryFilter.OpOptions.GE,
						value: "0")
				},
				Order = new List<QueryOrder>
				{
					new QueryOrder(
						asc: false,
						field: QueryOrder.FieldOptions.Created
						)
				},
			};

			var lobbyListQueryResponse = await LobbyService.Instance.QueryLobbiesAsync(options);
			var result = lobbyListQueryResponse.Results;
			
#if UNITY_EDITOR
			string text = "---Lobby List---\n";
			foreach (var lobby in result)
			{
				text += $"{lobby.Id} {lobby.Name} {lobby.AvailableSlots}/{lobby.MaxPlayers}\n";
				text += $"	GameMode: {lobby.Data["GameMode"].Value}\n";
				text += $"	GameStart: {lobby.Data["GameStart"].Value}\n";
				text += $"	RelayJoinCode: {lobby.Data["RelayJoinCode"].Value}\n";
			}
			text += "------";
			Debug.Log(text);
#endif
			return result;
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.GetLobbyList LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}" +
				$"Message: {e.Message}");

			return null;
		}
		catch (Exception e)
		{
			Debug.LogError($"UGSLobbyManager.GetLobbyList Exception. message: {e.Message}");

			return null;
		}
	}

	public static async Awaitable<Lobby> JoinLobbyById(string lobbyId, string password)
	{
		var options = new JoinLobbyByIdOptions
		{
			Player = GetPlayer(),
			Password = password
		};
		
		return await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options);
	}

	/*
	 * 
	 */
	public static async Awaitable<(bool result, Lobby lobby)> GetLobbyById(string id)
	{
		try
		{
			var lobby = await LobbyService.Instance.GetLobbyAsync(id);

#if UNITY_EDITOR
			var players = lobby.Players;

			string log = $"===Lobby updated({lobby.Version})===";

			log += "---Player List---\n";
			foreach (var player in players)
			{
				log += $"{player.Profile.Name} {PlayerName} {player.Id} {player.Joined} {player.ConnectionInfo}\n";
			}
			log += "------\n";

			log += "---Lobby Data---\n";

			foreach (var data in lobby.Data)
			{
				log += $"{data.Key} {data.Value}\n";
			}
			log += "------";

			Debug.Log(log);
#endif

			return (true, lobby);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.GetLobbyById LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
			return (false, null);
		}
	}

	/*
	 * host에게 로비를 삭제할 권한이 줄 것인지
	 */
	public static async void DeleteLobby(Lobby lobby)
	{
		if (!IsLobbyHost(lobby))
		{
			return;
		}
		
		try
		{
			await LobbyService.Instance.DeleteLobbyAsync(lobby.Id);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.DeleteLobby LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
		}
	}

	/*
	 * host가 나가면 남아있는 player 중에서 host로 무작위 지정
	 * 마지막 player가 나가면 lobby 자동으로 삭제됨
	 */
	public static async Awaitable<(bool result, string reason)> RemovePlayer(Lobby lobby, string playerId = "")
	{
		try
		{
			await LobbyService.Instance.RemovePlayerAsync(
				lobby.Id, 
				playerId == "" ? AuthenticationService.Instance.PlayerId : playerId);

			return (true, "ok");
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.RemovePlayer LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
			return (false, e.Message);
		}
	}

	public static async Awaitable<Lobby> Reconnect(Lobby lobby)
	{
		return await LobbyService.Instance.ReconnectToLobbyAsync(lobby.Id);
	}

	public static async Awaitable<(bool result, List<string>)> GetJoinedLobbie()
	{
		try
		{
			return (true, await LobbyService.Instance.GetJoinedLobbiesAsync());
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.GetJoinedLobbie LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
			return (false, null);
		}
	}
}
