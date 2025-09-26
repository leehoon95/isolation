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
	//[SerializeField] UserInfoSO _userInfo;

	UserInfoHolder _userInfoHolder;

	void Start()
	{
		_userInfoHolder = FindAnyObjectByType<UserInfoHolder>();

		// Lobby Scene에서 실행하지 않은 경우
		if (_userInfoHolder == null )
		{
			var obj = new GameObject("[UserInfoHolder(Debugging)]");
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

		_tcpClient.OnReceived += OnTCPDataReceived;
	}

	void OnClickCreateSession()
	{
		M_RequestCreateSession rcr = new();

		_uiso.DialogManager.OpenDialog_CR();
	}

	void OnClickSettings()
	{

	}

	void OnClickRefresh()
	{


		M_RequestSessionList rrl = new();

		rrl.Filter = 1;

		var data = rrl.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionList, data);
	}

	void OnClickExit()
	{
		_ = SceneManager.LoadSceneAsync("LoginScene");
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

		M_RequestCreateSession rcr = new();

		rcr.SessionName = sessionName;
		rcr.Password = password;

		print($"Request to create session {sessionName} / {password}");

		var data = rcr.ToByteArray();

		_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionCreate, data);

		_uiso.DialogManager.CloseDialog();
	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			// Disconnected from server.

			await Awaitable.MainThreadAsync();

			_userInfoHolder.UserInfo.MessageFromPreviousScene = "Disconnected from the server.";
			_ = SceneManager.LoadSceneAsync("LoginScene");

			return;
		}

		LobbyMessage_Type type = (LobbyMessage_Type)BitConverter.ToInt32(buffer, 4);

		Debug.LogWarning($"OnDataReceivecFromServer({type}, data, {length}");


		if (type == LobbyMessage_Type.ResponseSessionList)
		{
			M_ResponseSessionList rsl;

			try
			{
				rsl = M_ResponseSessionList.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogException(e, this);
				return;
			}

			if (rsl.Count == 0 || rsl.List.Count == 0)
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
		else if (type == LobbyMessage_Type.ResponseSessionCreate)
		{
			M_ResponseCreateSession rcr;

			try
			{
				rcr = M_ResponseCreateSession.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogException(e, this);
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

			_userInfoHolder.UserInfo.IsHost = true;
			_userInfoHolder.UserInfo.HostingSessionIndex = sessionIndex;

			M_RequestEnterSession res = new();

			res.SessionIndex = sessionIndex;
			res.Host = true;

			var data = res.ToByteArray();

			_ = _tcpClient.SendDataAsync((int)LobbyMessage_Type.RequestSessionEnter, data);
		}
		else if (type == LobbyMessage_Type.ResponseSessionEnter)
		{
			M_ResponseEnterSession res;

			try
			{
				res = M_ResponseEnterSession.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogException(e, this);
				return;
			}

			await Awaitable.MainThreadAsync();
			_uiso.ShowNotification($"M_ResponseEnterSession {res.Result}, {res.Reason}");

			if (res.Result)
			{
				
				_ = SceneManager.LoadSceneAsync("NGOTestScene");
			}
			else
			{
				_uiso.ShowNotification($"세션에 들어갈 수 없습니다. {res.Reason}");
			}
		}
	}

	void OnDestroy()
	{
		print("Lobby ondestroy");
		_uiso.OnClickCreateSession -= OnClickCreateSession;
		_uiso.OnClickSettings -= OnClickSettings;
		_uiso.OnClickRefresh -= OnClickRefresh;
		_uiso.OnClickExit -= OnClickExit;
		_uiso.OnSendMessage -= OnSendMessage;
		_uiso.DialogManager.RemoveOnOk_CR(OnCreateSession);
		_uiso.OnCancelDialog -= OnCancelDialog;

		//_tcpClient.RemoveReceiveListner(OnTCPDataReceived);
		_tcpClient.OnReceived -= OnTCPDataReceived;
	}
}
