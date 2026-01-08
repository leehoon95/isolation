using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UGSLobbyManager : MonoBehaviour
{
	static Lobby _cacheLobby;
	static string _nickname;
	static string _personalColor;

	public static string nickname
	{
		get => _nickname;
		set
		{
			_nickname = value;
			var p = GetPlayer();
			p.Data["Nickname"].Value = value;
		}
	}
	public static string PersonalColor
	{
		get => _personalColor;
		set
		{
			_personalColor = value;
			var p = GetPlayer();
			p.Data["PersonalColor"].Value = value;
		}
	}

	static Player _player;

	//public static Action<ILobbyChanges> OnLobbyChanged;
	//public static Action OnKickedFromLobby;
	//public static Action<LobbyEventConnectionState> OnLobbyEventConnectionStateChanged;


	static Player GetPlayer()
	{
		if (_player == null)
		{
			_player = new Player
			{
				Data = new Dictionary<string, PlayerDataObject>
				{
					{"Nickname",
						new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _nickname)},
					{"PersonalColor",
						new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, _personalColor)},
				},
				//Profile = new PlayerProfile(_nickname)
			};
		}

		return _player;
	}

	static void CreatePlayer()
	{
		
	}

	/*
	 * 
	 */
	//static void InvokeOnLobbyChanged(ILobbyChanges changes) => OnLobbyChanged?.Invoke(changes);
	//static void InvokeOnKickedFromLobby() => OnKickedFromLobby?.Invoke();
	//static void InvokeOnLobbyEventConnectionStateChanged(LobbyEventConnectionState changes)
	//	=> OnLobbyEventConnectionStateChanged?.Invoke(changes);


	public static async Task<(bool, Lobby, ILobbyEvents)> CreateLobby(
		string lobbyName,
		int maxPlayers,
		string relayJoinCode,
		LobbyEventCallbacks callbacks = null,
		string password = null)
	{
		/*
		 * Player: 로비 만드는 player 정보
		 * IsPrivate: 로비를 공개한 것인가. 비공개면 LobbyCode를 이용해서 다른 사용자가 로비에 join 가능.
		 * Data: 로비에 적용하는 커스텀 게임 속성(map name, game type etc.)
		 */
		var createOptions = new CreateLobbyOptions
		{
			Player = GetPlayer(),
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

			ILobbyEvents cb = null;
			if (callbacks != null)
			{
				cb = await LobbyService.Instance.SubscribeToLobbyEventsAsync(lobby.Id, callbacks);
			}

#if UNITY_EDITOR
			Debug.Log("Created Lobby Info\n" +
				$"id: {lobby.Id}\n" +
				$"private: {lobby.IsPrivate}\n" +
				$"max players: {lobby.MaxPlayers}\n" +
				$"created: {lobby.Created}"
				);
#endif

			return (true, lobby, cb);
		}
		catch (ArgumentNullException e)
		{
			// Thrown when lobbyName is null or only contains whitespaces.
			GLogger.LogError($"UGSLobbyManager.CreateLobby ArgumentNullException. message: {e.Message}");
			return (false, null, null);
		}
		catch (InvalidOperationException e)
		{
			// Thrown when maxPlayers is less than one.
			GLogger.LogError($"UGSLobbyManager.CreateLobby InvalidOperationException. message: {e.Message}");
			return (false, null, null);
		}
		catch (LobbyServiceException e)
		{
			// Thrown when the lobby service returns an error.
			GLogger.LogError($"UGSLobbyManager.CreateLobby LobbyServiceException. message: {e.Message}");
			switch (e.Reason)
			{
				case LobbyExceptionReason.AlreadySubscribedToLobby:
					Debug.LogWarning($"Already subscribed to lobby({lobbyName}). We did not need to try and subscribe again. Exception Message: {e.Message}");
					break;
				case LobbyExceptionReason.SubscriptionToLobbyLostWhileBusy:
					Debug.LogError($"Subscription to lobby events was lost while it was busy trying to subscribe. Exception Message: {e.Message}");
					break;
				case LobbyExceptionReason.LobbyEventServiceConnectionError:
					Debug.LogError($"Failed to connect to lobby events. Exception Message: {e.Message}");
					break;
			}

			return (false, null, null);
		}
	}

	public static async Task MaintainLobbyAlive(Lobby lobby)
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

	public static async Task<List<Lobby>> GetLobbyList(bool isAvailableSlot = false)
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
						value: isAvailableSlot ? "1" : "0")
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

			return result;
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.GetLobbyList LobbyServiceException. Reason: {e.Reason}" +
				$"Message: {e.Message}");

			return null;
		}
		catch (Exception e)
		{
			Debug.LogError($"UGSLobbyManager.GetLobbyList Exception. message: {e.Message}");

			return null;
		}
	}

	public static async Task<(Lobby, string)> JoinLobbyById(string lobbyId, string password)
	{
		var options = new JoinLobbyByIdOptions
		{
			Player = GetPlayer(),
			Password = password
		};

		try
		{
			return (await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId, options), "ok");
		}
		catch (LobbyServiceException e)
		{
			GLogger.LogError($"UGSLobbyManager.JoinLobbyById Exception. Reason: {e.Reason}");
			string reason = "unknown";

			switch(e.Reason)
			{
				case LobbyExceptionReason.LobbyFull: reason = "lobbyFull";
					break;
				default:
					reason = e.Reason.ToString(); 
					break;
			}

			return (null, reason);
		}
	}

	public static async Task<(bool result, Lobby lobby)> GetLobbyById(string id)
	{
		try
		{
			var lobby = await LobbyService.Instance.GetLobbyAsync(id);

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
		if (lobby == null)
		{
			return;
		}

		if (lobby.HostId != AuthenticationService.Instance.PlayerId)
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
	 * host가 다른 player를 kick할 때, 또는 player 스스로 나갈 때
	 * host가 나가면 남아있는 player 중에서 host로 무작위 지정
	 * 마지막 player가 나가면 lobby 자동으로 삭제됨
	 */
	public static async Task<(bool result, string reason)> RemovePlayer(Lobby lobby, string playerId = "")
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

	//public static async Task MigrateHost(Lobby from, string to)
	//{

	//	try
	//	{
	//		var joinedLobby = await LobbyService.Instance.UpdateLobbyAsync(from.Id,
	//			new UpdateLobbyOptions
	//			{
	//				HostId = to,
	//			});
	//	}
	//	catch (LobbyServiceException e)
	//	{
	//		Debug.LogError($"UGSLobbyManager.MigrateHost LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
	//	}
	//}

	public static async Task<Lobby> Reconnect(Lobby lobby)
	{
		return await LobbyService.Instance.ReconnectToLobbyAsync(lobby.Id);
	}

	public static async Task<(bool result, List<string>)> GetJoinedLobby()
	{
		try
		{
			var lobbyList = await LobbyService.Instance.GetJoinedLobbiesAsync();
			if (lobbyList == null || lobbyList.Count == 0)
			{
				GLogger.LogWarning("GetJoinedLobby 참가한 로비는 없다");
				return (false, null);
			}

			return (true, lobbyList);
		}
		catch (LobbyServiceException e)
		{
			Debug.LogError($"UGSLobbyManager.GetJoinedLobbie LobbyServiceException. errorCode: {e.ErrorCode}. message: {e.Message}");
			return (false, null);
		}
	}

	public static string LobbyInfo(Lobby lobby)
	{
		if (lobby == null)
		{
			return "";
		}

		string text = $"=== Lobby Info({lobby.Id}) ===\n";

		text += $"--- Lobby updated(version: {lobby.Version}) ---\n";

		text += "--- Lobby Data ---\n";
		foreach (var data in lobby.Data)
		{
			text += $"{data.Key}, {data.Value.Value}\n";
		}

		text += "--- Lobby Players ---\n";
		foreach (var player in lobby.Players)
		{
			text += $"{player.Id}, Profile Name: {player?.Profile?.Name}\n"
				+ $"	PlayerName: {player.Data["PlayerName"].Value}, PlayerLevel: {player.Data["PlayerLevel"].Value}\n"
				+ $"	joined: {player.Joined}, connection Info: {player?.ConnectionInfo}\n"
				+ $"	LastUpdated: {player?.LastUpdated}, Allocation ID: {player?.AllocationId}\n";
		}
		text += "--- Lobby Info ---\n";

		return text;
	}
}
