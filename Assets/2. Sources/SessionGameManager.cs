using UnityEngine;

public class SessionGameManager : MonoBehaviour
{
	UISessionSO _uiso;
	TCPClientSO _tcpClient;
	PlayerInfoSO _playerInfo;

	void Awake()
	{
		if (FindAnyObjectByType<UISessionSOHolder>() == null)
		{
			var obj = new GameObject("[UI Session Holder]");
			obj.AddComponent<UISessionSOHolder>();
		}

		if (FindAnyObjectByType<TCPClientHolder>() == null)
		{
			var obj = new GameObject("[TCP Client Holder]");
			obj.AddComponent<TCPClientHolder>();
			DontDestroyOnLoad(obj);
		}

		if (FindAnyObjectByType<PlayerInfoHolder>() == null)
		{
			var obj = new GameObject("[Player Info Holder]");
			obj.AddComponent<PlayerInfoHolder>();
			DontDestroyOnLoad(obj);
		}
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientHolder>().Data;
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;

		_uiso.OnSubmitMessage += OnSubmitMessage;
	}

	void OnSubmitMessage(string message)
	{
		GLogger.Log($"message: {message}");
		_uiso.AddMessage(message, Color.magenta);
	}
}
