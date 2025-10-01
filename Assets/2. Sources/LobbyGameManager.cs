using Google.Protobuf;
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LobbyManager : MonoBehaviour
{
	[SerializeField] UILobbySO _uiso;
	[SerializeField] TCPClientSO _tcpClient;

	UserInfoHolder _userInfoHolder;
	DateTime _lastRefreshTime = DateTime.MinValue;

	void Start()
	{
		_userInfoHolder = FindAnyObjectByType<UserInfoHolder>();

		// Lobby Scene에서 실행하지 않은 경우
		if (_userInfoHolder == null )
		{
			var obj = new GameObject("[UserInfoHolder(lobby)]");
			obj.AddComponent<UserInfoHolder>();

			_userInfoHolder = obj.GetComponent<UserInfoHolder>();
			_userInfoHolder.UserInfo.Debugging = true;
			_userInfoHolder.UserInfo.UserNickname = "TestName001";
			_userInfoHolder.UserInfo.MessageFromPreviousScene = "";
		}

		_uiso.OnClickCreateSession += OnClickCreateSession;
		_uiso.OnClickSettings += OnClickSettings;
		_uiso.OnClickRefresh += OnClickRefresh;
		_uiso.OnClickExit += OnClickExit;
		_uiso.OnSendMessage += OnSendMessage;
		_uiso.DialogManager.AddOnOk_CR(OnCreateSession);
		_uiso.OnCancelDialog += OnCancelDialog;

		_uiso.OnClickSession += OnClickSession;

		_tcpClient.OnReceived += OnTCPDataReceived;

		OnClickRefresh();
	}

	void OnClickCreateSession()
	{
		_uiso.DialogManager.OpenDialog_CR();
	}

	void OnClickSettings()
	{

	}

	async void OnClickRefresh()
	{
		var duration = DateTime.Now - _lastRefreshTime;

		if (duration.TotalMilliseconds < 1000)
		{
			return;
		}

#if UNITY_EDITOR
		if (_userInfoHolder.UserInfo.Debugging)
		{
			return;
		}
#endif

		M_RequestSessionList rrl = new();

		rrl.Filter = 1;

		var data = rrl.ToByteArray();

		await _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionList, data);

		_lastRefreshTime = DateTime.Now;
	}

	void OnClickExit()
	{
		CleanEvent();

		M_RequestLobbyExit rle = new();
		rle.Reason = "ok";

		var data = rle.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestLobbyExit, data);

		_ = SceneManager.LoadSceneAsync("LoginScene");
	}

	void OnClickSession(int sessionIndex)
	{
		M_RequestSessionEntry rcr = new();

		rcr.SessionIndex = sessionIndex;

		var data = rcr.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionEntry, data);

		//_userInfoHolder.UserInfo.StartHost = false;
	}

	void OnSendMessage(string message)
	{

	}

	void OnCancelDialog()
	{
		_uiso.DialogManager.CloseDialog();
	}

	void OnCreateSession(string sessionName, string password)
	{
		print($"OnCreateRoom {sessionName} {password}");

		if (sessionName == null)
		{
			return;
		}

		if (sessionName.Length < 1)
		{
			_uiso.ShowNotification("방 이름은 두 글자 이상이어야 합니다");
			return;
		}

		M_RequestSessionCreation rcr = new();
		rcr.SessionName = sessionName;
		rcr.Password = password;

		print($"Request to create session {sessionName} / {password}");

		var data = rcr.ToByteArray();
		_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionCreation, data);
		_uiso.DialogManager.CloseDialog();
	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		LobbyMessage_Type type = (LobbyMessage_Type)BitConverter.ToInt32(buffer, 4);
		Debug.Log($"LobbyGameManager.OnDataReceivecFromServer(type: {type}, len: {length})");

		if (length == 0)
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			_userInfoHolder.UserInfo.MessageFromPreviousScene = "Disconnected from the server.";
			_ = SceneManager.LoadSceneAsync("LoginScene");

			return;
		}

		if (type == LobbyMessage_Type.ResponseSessionList)
		{
			M_ResponseSessionList rsl;

			try
			{
				rsl = M_ResponseSessionList.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogError($"M_ResponseSessionList ParseFrom error: {e.Message}");
				return;
			}

			if (rsl.List.Count == 0)
			{
				_uiso.ShowNotification("공개된 Session이 없습니다");
				return;
			}
			else
			{
				await Awaitable.MainThreadAsync();

				var list = rsl.List;

				_uiso.ResizeSessionList(list.Count);

				for (int i = 0; i < list.Count; ++i)
				{
					var sinfo = list[i];
					_uiso.SetSessionInfoIndex(
						i,
						sinfo.SessionIndex,
						sinfo.SessionName,
						sinfo.MaxClientCount,
						sinfo.ClientCount,
						sinfo.Password,
						sinfo.JoinCode);
				}
			}
		}
		else if (type == LobbyMessage_Type.ResponseSessionCreation)
		{
			M_ResponseSessionCreation rcr;

			try
			{
				rcr = M_ResponseSessionCreation.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogError($"M_ResponseSessionCreation ParseFrom error: {e.Message}");
				return;
			}

			var sessionIndex = rcr.SessionIndex;
			var reason = rcr.Reason;

			await Awaitable.MainThreadAsync();
			if (sessionIndex < 0)
			{
				_uiso.ShowNotification($"Failed to create room. {reason}");
				return;
			}

			_uiso.ShowNotification($"Room created {sessionIndex}, {reason}");

			M_RequestSessionEntry res = new();
			res.SessionIndex = sessionIndex;

			var data = res.ToByteArray();
			_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionEntry, data);

			//_userInfoHolder.UserInfo.StartHost = true;
		}
		else if (type == LobbyMessage_Type.ResponseSessionEntry)
		{
			M_ResponseSessionEntry res;

			try
			{
				res = M_ResponseSessionEntry.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogError($"M_ResponseSessionEntry ParseFrom error: {e.Message}");
				return;
			}

			await Awaitable.MainThreadAsync();
			

			if (res.Result)
			{
				
				_ = SceneManager.LoadSceneAsync("NGOTestScene");
			}
			else
			{
				Debug.LogError($"M_ResponseEnterSession {res.Result}, {res.Reason}");
				//_uiso.ShowNotification($"세션에 들어갈 수 없습니다. {res.Reason}");
			}
		}
	}

	void CleanEvent()
	{
		_uiso.OnClickCreateSession -= OnClickCreateSession;
		_uiso.OnClickSettings -= OnClickSettings;
		_uiso.OnClickRefresh -= OnClickRefresh;
		_uiso.OnClickExit -= OnClickExit;
		_uiso.OnSendMessage -= OnSendMessage;
		_uiso.DialogManager.RemoveOnOk_CR(OnCreateSession);
		_uiso.OnCancelDialog -= OnCancelDialog;
		_uiso.OnClickSession -= OnClickSession;

		_tcpClient.OnReceived -= OnTCPDataReceived;
	}
}
