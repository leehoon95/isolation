using Google.Protobuf;
using System;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LoginGameManager : MonoBehaviour
{
	[SerializeField] TCPClientSO _tcpClient;
	[SerializeField] UILoginSO _uiso;
	[SerializeField] SaveDataLoader _sdl;
	[SerializeField] UserInfoSO _userInfo;

	void Start()
	{
		_uiso.OnLoginEnter += OnLoginEnter;
		_uiso.OnDisconnect += OnDisconnect; // disconnect test

		//_tcpClient.AddReceiveListner(OnTCPDataReceived);
		_tcpClient.OnReceived += OnTCPDataReceived;

		if (_userInfo.MessageFromPreviousScene != null)
		{
			_uiso.ShowNotification(_userInfo.MessageFromPreviousScene);
			_userInfo.MessageFromPreviousScene = null;
		}

		if (!_tcpClient.Connnected)
		{
			TryConnectToServer();
		}
	}

	async void TryConnectToServer()
	{
		_uiso.DialogManager.SetActive_Ok(false);
		_uiso.DialogManager.RemoveOnOk_Ok(TryConnectToServer);

		var res = await _tcpClient.ConnectToServer();

		if (res)
		{
			_uiso.ShowNotification("서버에 연결됨");
		}
		else
		{
			_uiso.DialogManager.SetActive_Ok(true);
			_uiso.DialogManager.SetTitle_Ok("오류");
			_uiso.DialogManager.SetContent_Ok("서버와 연결할 수 없습니다");
			_uiso.DialogManager.SetOkButtonText_Ok("재시도");
			_uiso.DialogManager.AddOnOk_Ok(TryConnectToServer);
		}
	}

	void OnLoginEnter(string nickname)
	{
		if (nickname == null)
		{
			return;
		}

		if (nickname.Length < 2)
		{
			_uiso.ShowNotification($"닉네임은 최소 두 글자 이상이어야 합니다.");
			return;
		}

		_userInfo.UserNickname = nickname;

		M_RequestLogin msg = new M_RequestLogin();
		msg.Nickname = nickname;

		var data = msg.ToByteArray();

		//_ = _ns.SendTCPDataAsync(PROTO_MessageType.RequestLogin, data);
		_ = _tcpClient.SendDataAsync((int)LoginMessage_Type.RequestLogin, data);
	}

	// test button
	void OnDisconnect()
	{
		//_ns.CloseConnection();
		_tcpClient.CloseConnection();
	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			await Awaitable.MainThreadAsync();

			TryConnectToServer();

			return;
		}

		LoginMessage_Type type = (LoginMessage_Type)BitConverter.ToInt32(buffer, 4);

		print($"OnDataReceivecFromServer({type}, data, {length}");
	
		if (type == LoginMessage_Type.ResponseLogin)
		{
			M_ResponseLogin msg;

			try
			{
				msg = M_ResponseLogin.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				Debug.LogException(e, this);
				return;
			}

			if (msg == null)
			{
				print("Failed to parse LoginResult.");
				return;
			}

			if (msg.Token > 0 && msg.Reason.ToLower().CompareTo("ok") == 0)
			{
				print("Allowed login request.");

				_userInfo.Token = msg.Token;

				print($"token : {_userInfo.Token}");

				await Awaitable.MainThreadAsync();

				//_ = SceneManager.LoadSceneAsync("LobbyScene");
				_ = SceneManager.LoadSceneAsync("LobbyScene");
			}
			else
			{
				await Awaitable.MainThreadAsync();
				_userInfo.UserNickname = null;
				_uiso.ShowNotification("Login failed. Please try again.");
				print($"Login request is Denied. (reason: {msg.Reason})");
			}

		}
	}

	void OnDestroy()
	{
		_uiso.OnLoginEnter -= OnLoginEnter;
		_uiso.OnDisconnect -= OnDisconnect;

		//_tcpClient.RemoveReceiveListner(OnTCPDataReceived);
		_tcpClient.OnReceived -= OnTCPDataReceived;
	}
}
