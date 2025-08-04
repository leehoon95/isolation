using System;
using Unity.VisualScripting;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
	[SerializeField] UILobbySO _uiSO;
	[SerializeField] TCPClientSO _tcpClient;
	 
	void Start()
	{
		_uiSO.OnClickCreateRoom += OnClickCreateRoom;
		_uiSO.OnClickSettings += OnClickSettings;
		_uiSO.OnClickRefresh += OnClickRefresh;
		_uiSO.OnClickExit += OnClickExit;
		_uiSO.OnSendMessage += OnSendMessage;

		_tcpClient.AddReceiveListner(OnTCPDataReceived);
	}

	void OnClickCreateRoom(int roomIndex)
	{

	}

	void OnClickSettings()
	{

	}

	void OnClickRefresh()
	{

	}

	void OnClickExit()
	{

	}

	void OnSendMessage(string message)
	{

	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		RM_Type type = (RM_Type)BitConverter.ToInt32(buffer, 4);

		print($"OnDataReceivecFromServer({type}, data, {length}");
		
		if (buffer == null && length == 0 )
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			// ...
		}
		else if (type == RM_Type.SmResponseRoomList)
		{
			RM_ResponseRoomList rrl = RM_ResponseRoomList.Parser.ParseFrom(buffer, 12, length - 12);

			if (rrl.Count == 0 || rrl.List.Count == 0)
			{
				print($"OnTCPDataReceived room list count ({rrl.Count} {rrl.List.Count})");

				return;
			}

			_uiSO.RefreshRommList(rrl);

		}
	}

	void OnDestroy()
	{
		_uiSO.OnClickCreateRoom -= OnClickCreateRoom;
		_uiSO.OnClickSettings -= OnClickSettings;
		_uiSO.OnClickRefresh -= OnClickRefresh;
		_uiSO.OnClickExit -= OnClickExit;
		_uiSO.OnSendMessage -= OnSendMessage;

		_tcpClient.RemoveReceiveListner(OnTCPDataReceived);
	}
}
