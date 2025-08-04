using Google.Protobuf;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginGameManager : MonoBehaviour
{
	//[SerializeField]
	//NetworkSynchronizer _ns;
	[SerializeField] TCPClientSO _tcpClient;
	[SerializeField] UILoginSO _uiLogin;
	[SerializeField] SaveDataLoader _sdl;
	[SerializeField] PlayerInfoSO _pinfo;

	Scene _syncScene;
	PhysicsScene2D _syncPS;

	void Start()
	{
		_uiLogin.OnLoginEnter += OnLoginEnter;
		_uiLogin.OnDisconnect += OnDisconnect;

		//_ns.OnReceivedTCP += OnDataReceivedFromServer;
		_tcpClient.AddReceiveListner(OnTCPDataReceived);
		_ = _tcpClient.ConnectToServer("172.23.12.33", 51010);

		StartCoroutine(CheckNetworkState("172.23.12.33", 51010)); // 172.23.12.33: wsl2 ¼­¹ö
	}

	IEnumerator CheckNetworkState(string server, int port)
	{
		yield return new WaitForSeconds(1f);

		while (true)
		{
			if (_tcpClient.Connnected)
			{
				yield return new WaitForSeconds(10f);
				continue;
			}

			var res = _tcpClient.ConnectToServer(server, port);
			//         if (_ns.Connected)
			//         {
			//             yield return new WaitForSeconds(10f);
			//             continue; // Already connected, wait for next check
			//}

			//         var res = _ns.ConnectToServer(server, port);

			while (res.Status != TaskStatus.RanToCompletion)
			{
				yield return new WaitForSeconds(1f);
			}

			if (res.Result)
			{
				print("Connected to the server.");
				_uiLogin.ShowNoticeOnTop("Connected to the server.");
				//var p = new Ping(server);

				//            if (p.isDone)
				//            {
				//                print($"ping: {p.time} ms");
				//                yield return new WaitForSeconds(5f);
				//            }
				//            else
				//            {
				//	yield return new WaitForSeconds(0.1f);
				//}

				yield return new WaitForSeconds(10f);
			}
			else
			{
				print("Trying to connect to server...");
				_uiLogin.ShowNoticeOnTop("Trying to connect to server...");
				yield return new WaitForSeconds(3f);
			}
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
