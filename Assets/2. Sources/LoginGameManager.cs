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

	PlayerInfoHolder _PlayerInfoHolder;

	void Start()
	{
		_PlayerInfoHolder = FindAnyObjectByType<PlayerInfoHolder>();

		if (_PlayerInfoHolder == null)
		{
			var obj = new GameObject("[User Info Holder]");
			obj.AddComponent<PlayerInfoHolder>();

			_PlayerInfoHolder = obj.GetComponent<PlayerInfoHolder>();
		}

		_uiso.OnLoginEnter += OnLoginEnter;
		_uiso.OnDisconnect += OnDisconnect;

		_tcpClient.OnReceived += OnTCPDataReceived;

		if (_PlayerInfoHolder.PlayerInfo.MessageFromPreviousScene != null)
		{
			_uiso.ShowNotification(_PlayerInfoHolder.PlayerInfo.MessageFromPreviousScene);
			_PlayerInfoHolder.PlayerInfo.MessageFromPreviousScene = null;
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

		if (_tcpClient.Connnected)
		{
			Debug.Log("TryConnectToServer connected");
			return;
		}

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

		_PlayerInfoHolder.PlayerInfo.PlayerNickname = nickname;

		M_RequestLogin msg = new M_RequestLogin();
		msg.Nickname = nickname;

		var data = msg.ToByteArray();

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

			_uiso.ShowNotification("서버와 연결이 끊어짐");
			TryConnectToServer();

			return;
		}

		LoginMessage_Type type = (LoginMessage_Type)BitConverter.ToInt32(buffer, 4);
		Debug.Log($"LoginGameManager.OnDataReceivecFromServer(type: {type}, len: {length})");

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

			await Awaitable.MainThreadAsync();

			if (msg.Result)
			{
				print("Login success");

				CleanEvent();
				//_ = SceneManager.LoadSceneAsync("LobbyScene");
				SceneManager.LoadScene("LobbyScene");
			}
			else
			{
				_PlayerInfoHolder.PlayerInfo.PlayerNickname = null;
				_uiso.ShowNotification("Login failed. Please try again.");
				print($"Login request is Denied. (reason: {msg.Reason})");
			}
		}
	}

	void CleanEvent()
	{
		_uiso.OnLoginEnter -= OnLoginEnter;
		_uiso.OnDisconnect -= OnDisconnect;

		_tcpClient.OnReceived -= OnTCPDataReceived;
	}
}
