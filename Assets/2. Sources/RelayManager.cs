using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using WebSocketSharp;

public class RelayManager
{
	//static RelayManager s_instance;

	//public static RelayManager Instance
	//{
	//	get
	//	{
	//		if (s_instance == null)
	//		{
	//			s_instance = new RelayManager();
	//		}

	//		return s_instance;
	//	}
	//	private set
	//	{
	//		s_instance = value;
	//	}
	//}

	static NetworkDriver _networkDriver;

	public static async Awaitable InitServices()
	{
		try
		{
			await UnityServices.InitializeAsync();

			if (!AuthenticationService.Instance.IsSignedIn)
			{
				await AuthenticationService.Instance.SignInAnonymouslyAsync();
			}

			Debug.LogWarning($"Player ID: {AuthenticationService.Instance.PlayerId}");
		}
		catch (Exception e)
		{
			Debug.LogException(e);
		}
	}

	/*
	 * The connection type must be one of the following options:
	 * udp
	 * dtls
	 * wss
	 */
	public static async Task<string> StartHostWithRelayAndGetJoinCode(int maxConnections, string connectionType)
	{
		await InitServices();

		/*
		 * IRelayService.CreateAllocationAsync
		 * 적절한 Relay 서버를 지역에서 골라 할당을 생성.
		 * Allocation 객체 안에는 host, join에 필요한 네트워크 정보, 토큰 등이 있음
		 */
		Allocation allocation;

		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(
				maxConnections: maxConnections);
		}
		catch (Exception e)
		{
			Debug.LogError($"Realy create allocation request failed {e.Message}");
			throw;
		}
		/*
		 * AllocationUtils.ToRelayServerData
		 * Unity Transport에 데이터를 넘기기위한 헬퍼 메서드 (Allocation 정보 -> RelayServerData 구조체)
		 * 
		 */
		NetworkManager.Singleton.GetComponent<UnityTransport>()
			.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		
		return NetworkManager.Singleton.StartHost() ? joinCode : null;
	}

	
	public static async Task<bool> StartClientWithRelay(string joinCode, string connectionType)
	{
		await InitServices();

		if (joinCode.IsNullOrEmpty())
		{
			Debug.LogError("joinCode is null or empty.");
			return false;
		}

	   var allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
		NetworkManager.Singleton.GetComponent<UnityTransport>()
			.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

		return NetworkManager.Singleton.StartClient();
	}
}
