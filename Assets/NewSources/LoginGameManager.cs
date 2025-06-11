using Google.Protobuf;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

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

	void Start()
	{
        _uiLogin.OnLoginEnter += OnLoginEnter;

        StartCoroutine(ConnectToServer());
	}

    IEnumerator ConnectToServer()
    {
        yield return new WaitForSeconds(1f);

        while (true)
        {
            var res = _ns.ConnectToServer();

            if (res.Result)
            {
                print("Connected to the server.");
                yield break;
			}
            else
            {
                print("Connecting to the server has failed.");
				yield return new WaitForSeconds(5f);
			}
        }
    }

    void OnLoginEnter(string nickname)
    {
        LoginMessage msg = new LoginMessage();
        msg.Nickname = nickname;

        var data = msg.ToByteArray();
        
       
        _ns.WriteAsync(MessageType.Login, data);
    }
}
