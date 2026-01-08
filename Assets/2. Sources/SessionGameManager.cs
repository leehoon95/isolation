using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionGameManager : MonoBehaviour
{
	UISessionSO _uiso;
	TCPClientSO _tcpClient;
	PlayerInfoSO _playerInfo;
	SessionParameterSO _sessionParameter;

	void Awake()
	{
		if (FindAnyObjectByType<UISessionSOHolder>() == null)
		{
			var obj = new GameObject("[UI Session Holder]");
			obj.AddComponent<UISessionSOHolder>();
		}

		if (FindAnyObjectByType<TCPClientSOHolder>() == null)
		{
			var obj = new GameObject("[TCP Client Holder]");
			obj.AddComponent<TCPClientSOHolder>();
			DontDestroyOnLoad(obj);
		}

		if (FindAnyObjectByType<PlayerInfoSOHolder>() == null)
		{
			var obj = new GameObject("[Player Info Holder]");
			obj.AddComponent<PlayerInfoSOHolder>();
			DontDestroyOnLoad(obj);
		}

		if (FindAnyObjectByType<SessionParameterSOHolder>() == null)
		{
			GLogger.LogError("SessionGameManger.Awake This session is invalid");
			SceneManager.sceneLoaded += SceneLoadedOnInvalidSession;
		}

		
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_sessionParameter = FindAnyObjectByType<SessionParameterSOHolder>().Data;

		_uiso.OnSubmitMessage += OnSubmitMessage;
	}

	void OnDisable()
	{
		SceneManager.sceneLoaded -= SceneLoadedOnInvalidSession;
	}

	void SceneLoadedOnInvalidSession(Scene scene, LoadSceneMode mode)
	{
		SceneManager.LoadScene("LobbyScene");
	}

	void OnSubmitMessage(string message)
	{
		GLogger.Log($"message: {message}");
		_uiso.AddMessage(message, Color.magenta);
	}
}
