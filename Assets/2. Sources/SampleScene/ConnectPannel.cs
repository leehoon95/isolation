using TMPro;
using UnityEngine;

public class ConnectPannel : MonoBehaviour
{
	[SerializeField]
	TMP_InputField adressInputField;
	[SerializeField]
	TMP_InputField portInputField;
	[SerializeField]
	TMP_Text networkStatus;
	[SerializeField]
	Server server;
	[SerializeField]
	ServerEventSO serverEventSO;

	public void OnConnect()
    {
        print("OnConnect");
        _ = server.ConnectToServer(adressInputField.text, int.Parse(portInputField.text));
	}

    public void OnDisconnect()
	{
        print("OnDisconnect");
        server.DisconnectFromServer();
	}

	public void OnSendProtocbufData()
	{
		print("OnSendProtocbufData");
		_ = server.SendProtobufDataToServer();
	}

    public string GetIPAddress() => adressInputField.text;
	public int GetPort() => int.Parse(portInputField.text);

	void Start()
    {
		serverEventSO.OnServerConnected += (string message) =>
		{
			print("OnServerConnected");
			networkStatus.text = "Connected to server!";
		};

		serverEventSO.OnServerDisconnected += (string message) =>
		{
			print("OnServerDisconnected");
			networkStatus.text = "Disconnected from server!";
		};
	}

    void Update()
    {
        
    }


}
