using Google.Protobuf;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginGameManager : MonoBehaviour
{
    [SerializeField]
    NetworkSynchronizer _ns;
    [SerializeField]
    UILogin _uiLogin;
    [SerializeField]
    MainThreadDispatcher _mainThreadDispatcher;
    [SerializeField]
    SaveDataLoader _saveDataLoader;

    Scene _syncScene;
    PhysicsScene2D _syncPS;

	void Start()
	{
        _uiLogin.OnLoginEnter += OnLoginEnter;
		_uiLogin.OnDisconnect += OnDisconnect;

        StartCoroutine(CheckNetworkState("127.0.0.1", 51010));

        _ns.OnReceived += OnDataReceivecFromServer;

        //CreateSceneParameters csp = new CreateSceneParameters(LocalPhysicsMode.Physics2D);
        //_syncScene = SceneManager.CreateScene("syncScene", csp);
        //_syncPS = _syncScene.GetPhysicsScene2D();


	}

    IEnumerator CheckNetworkState(string server, int port)
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            var res = _ns.ConnectToServer(server, port);
            
            while (res.Status != TaskStatus.RanToCompletion)
            {
                yield return new WaitForSeconds(0.5f);
            }

			if (res.Result)
			{
				print("Connected to the server.");
                var p = new Ping(server);

                if (p.isDone)
                {
                    print($"ping: {p.time} ms");
                    yield return new WaitForSeconds(5f);
                }
                else
                {
					yield return new WaitForSeconds(0.1f);
				}
			}
			else
			{
                print("Trying to connect to server...");
				_uiLogin.NoticeOnTop("Trying to connect to server...");
				yield return new WaitForSeconds(3f);
			}
        }
    }

	void OnLoginEnter(string nickname)
    {
		PROTO_RequestLogin msg = new PROTO_RequestLogin();
        msg.Nickname = nickname;

        var data = msg.ToByteArray();
        
       
        _ns.WriteAsync(PROTO_MessageType.RequestLogin, data);
    }

    void OnDisconnect()
    {
        _ns.CloseConnection();

	}

    void OnDataReceivecFromServer(byte[] buffer, int length)
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
            }
            else
            {
                print("Denied login request.");
            }

        }
    }
}
