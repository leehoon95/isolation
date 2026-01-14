using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using WebSocketSharp;

public class UGSRelayManager
{
	/*
	 * The connection type must be one of the following options:
	 * udp
	 * dtls
	 * wss
	 * 
	 * return (result, joincode(or reason if failed))
	 */
	public static async Task<(bool, string)> StartHostAndGetJoinCode(
		int maxConnections,
		string connectionType)
	{
		/*
		 * IRelayService.CreateAllocationAsync
		 * 적절한 Relay 서버를 지역에서 골라 할당을 생성.
		 * Allocation 객체 안에는 host, join에 필요한 네트워크 정보, 토큰 등이 있음
		 */
		Allocation allocation = null;
		
		try
		{
			allocation = await RelayService.Instance.CreateAllocationAsync(
				maxConnections: maxConnections);
		}
		catch (RelayServiceException rse)
		{
			GLogger.LogError("RealyManager.StartHostWithRelayAndGetJoinCode Exception(RelayServiceException)" +
				$"reason: {rse.Reason.ToString()}");

			return (false, null);
		}
		catch (Exception e)
		{
			GLogger.LogError($"RealyManager.StartHostWithRelayAndGetJoinCode " +
				$"Exception: {e.Message}");

			return (false, null);
		}

		/*
		 * AllocationUtils.ToRelayServerData
		 * Unity Transport에 데이터를 넘기기위한 헬퍼 메서드 (Allocation 정보 -> RelayServerData 구조체)
		 * 
		 */
		NetworkManager.Singleton.GetComponent<UnityTransport>()
			.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));
		var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
		
		return (true, NetworkManager.Singleton.StartHost() ? joinCode : null);
	}

	/*
	 * return (result, reason)
	 */
	public static async Task<bool> StartClient(string joinCode, string connectionType)
	{
		if (joinCode.IsNullOrEmpty())
		{
			Debug.LogError("joinCode is null or empty.");
			return false;
		}

#if UNITY_EDITOR
		NetworkManager.Singleton.LogLevel = Unity.Netcode.LogLevel.Developer;
#else
		NetworkManager.Singleton.LogLevel = Unity.Netcode.LogLevel.Nothing;
#endif

		JoinAllocation allocation = null;

		try
		{
			allocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
			
			NetworkManager.Singleton.GetComponent<UnityTransport>()
				.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, connectionType));

			if (NetworkManager.Singleton.StartClient())
			{
				Debug.Log("start client success");
				return true;
			}
			else
			{
				Debug.Log("start client failed");
				return false;
			}
		}
		catch (RelayServiceException rse)
		{
			Debug.Log("RelayManager.StartHostWithRelayAndGetJoinCode Exception(RelayServiceException)" +
				$"reason: {rse.Reason.ToString()}");
			
			return false;
		}
		catch (Exception e)
		{
			Debug.LogError($"RelayManager.StartHostWithRelayAndGetJoinCode " +
				"Exception: {e.Message}");
			
			return false;
		}
	}

	//public static int GetConnectedClientCount()
	//{
		
	//}
}
