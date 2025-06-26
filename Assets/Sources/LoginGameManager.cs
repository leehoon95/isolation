using Google.Protobuf;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginGameManager : MonoBehaviour
{
    [SerializeField]
    NetworkSynchronizer _ns;
    [SerializeField]
    UILogin _uiLogin;
    [SerializeField]
    SaveDataLoader _sdl;
    [SerializeField]
    PlayerInfoSO _pinfo;

    Scene _syncScene;
    PhysicsScene2D _syncPS;

	void Start()
	{
        _uiLogin.OnLoginEnter += OnLoginEnter;
		_uiLogin.OnDisconnect += OnDisconnect;
		_uiLogin.OnSendUDPData += OnSendUDPData;

		

        _ns.OnReceivedTCP += OnDataReceivedFromServer;
		_ns.OnReceivedUDP += OnDataReceivedFromServerUDP;

		StartCoroutine(CheckNetworkState("172.23.12.33", 51010)); // 172.23.12.33: wsl2 ¼­¹ö

		//CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
		//_syncScene = SceneManager.CreateScene("syncScene", csp);
		//_syncPS = _syncScene.GetPhysicsScene2D();


	}

    IEnumerator CheckNetworkState(string server, int port)
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            if (_ns.Connected)
            {
                yield return new WaitForSeconds(10f);
                continue; // Already connected, wait for next check
			}

            var res = _ns.ConnectToServer(server, port);
            
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
		_pinfo.SetNickname(nickname);

		PROTO_RequestLogin msg = new PROTO_RequestLogin();
        msg.Nickname = nickname;

        var data = msg.ToByteArray();
       
        _ = _ns.WriteByteAsync(PROTO_MessageType.RequestLogin, data);
    }

    void OnDisconnect()
    {
        _ns.CloseConnection();

	}

    void OnDataReceivedFromServer(byte[] buffer, int length)
    {
		PROTO_MessageType type = (PROTO_MessageType)BitConverter.ToInt32(buffer, 4);

		print($"OnDataReceivecFromServer({type}, data, {length}");
		
		if (type == PROTO_MessageType.LoginResult)
        {
			
			PROTO_LoginResult msg = PROTO_LoginResult.Parser.ParseFrom(buffer, 12, length - 12);
			if (msg == null)
            {
                print("Failed to parse LoginResult.");
                return;
            }

            if (msg.Result)
            {
                print("Allowed login request.");

                MainThreadDispatcher.Enqueue(() =>
                {
                    _ns.OnReceivedTCP -= OnDataReceivedFromServer;
					_ = SceneManager.LoadSceneAsync("TestScene");
				});
            }
            else
            {
                MainThreadDispatcher.Enqueue(() =>
                {
                    _uiLogin.ShowNoticeOnTop("Login failed. Please try again.");
                });
				print("Denied login request.");
            }

        }
    }

    void OnDataReceivedFromServerUDP(byte[] buffer)
    {
        string msg = new string(System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length));

		print($"OnDataReceivedFromServerUDP: {msg}");
	}

    void OnSendUDPData()
    {
        print("OnSendUDPData() called");
		_ = _ns.SendUDPDataAsync(Encoding.UTF8.GetBytes("hello from client!----"));

	}
}
