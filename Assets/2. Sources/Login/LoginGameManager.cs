using Google.Protobuf;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginGameManager : MonoBehaviour
{
	[SerializeField] TCPClientSO _tcpClient;
	[SerializeField] UILoginSO _uiLogin;
	[SerializeField] SaveDataLoader _sdl;
	[SerializeField] PlayerInfoSO _pinfo;

	Scene _syncScene;
	PhysicsScene2D _syncPS;
	Coroutine _connectToServerCoroutin;

	void Start()
	{
		_uiLogin.OnLoginEnter += OnLoginEnter;
		_uiLogin.OnDisconnect += OnDisconnect;

		//_ns.OnReceivedTCP += OnDataReceivedFromServer;
		_tcpClient.AddReceiveListner(OnTCPDataReceived);

		_connectToServerCoroutin = StartCoroutine(CheckNetworkState());
	}

	IEnumerator CheckNetworkState()
	{
		yield return null;

		var res = _tcpClient.ConnectToServer();

		while (res.Status != TaskStatus.RanToCompletion)
		{
			yield return new WaitForSeconds(0.1f);
		}

		if (!res.Result)
		{
			_uiLogin.SetDialogType(UIDialogType.Ok);
			_uiLogin.SetDialogTitleText("오류");
			_uiLogin.SetDialogContentText("서버와 연결할 수 없습니다.");
			_uiLogin.SetDialogOKButtonText("재시도");
			_uiLogin.SetDialogOKButtonOnClickListner(TryConnectToServer);
			_uiLogin.ShowDialog(true);
			//StopCoroutine(_connectToServerCoroutin);
		}

	}

	async void TryConnectToServer()
	{
		_uiLogin.ShowDialog(false);

		var res = await _tcpClient.ConnectToServer();

		if (res)
		{
			_uiLogin.ShowNoticeOnTop("서버에 연결됨");
		}
		else
		{
			_connectToServerCoroutin = StartCoroutine(CheckNetworkState());
		}
	}

	void OnLoginEnter(string nickname)
	{
		_pinfo.Nickname = nickname;

		LM_RequestLogin msg = new LM_RequestLogin();
		msg.Nickname = nickname;

		var data = msg.ToByteArray();

		//_ = _ns.SendTCPDataAsync(PROTO_MessageType.RequestLogin, data);
		_ = _tcpClient.SendDataAsync((int)LM_Type.CmRequestLogin, data);
	}

	void OnDisconnect()
	{
		//_ns.CloseConnection();
		_tcpClient.CloseConnection();
	}

	async Awaitable OnTCPDataReceived(byte[] buffer, int length)
	{
		LM_Type type = (LM_Type)BitConverter.ToInt32(buffer, 4);

		print($"OnDataReceivecFromServer({type}, data, {length}");

		if (type == LM_Type.SmResponseLogin)
		{
			LM_ResponseLogin msg = LM_ResponseLogin.Parser.ParseFrom(buffer, 12, length - 12);
			if (msg == null)
			{
				print("Failed to parse LoginResult.");
				return;
			}

			if (msg.Token > 0)
			{
				print("Allowed login request.");

				_pinfo.Token = msg.Token;

				print($"token : {_pinfo.Token}");

				_tcpClient.RemoveReceiveListner(OnTCPDataReceived);

				await Awaitable.MainThreadAsync();
				
				_ = SceneManager.LoadSceneAsync("TestScene");
			}
			else
			{
				await Awaitable.MainThreadAsync();

				_uiLogin.ShowNoticeOnTop("Login failed. Please try again.");
				print("Denied login request.");
			}

		}
	}

	void OnDestroy()
	{
		_uiLogin.OnLoginEnter -= OnLoginEnter;
		_uiLogin.OnDisconnect -= OnDisconnect;
	}
}
