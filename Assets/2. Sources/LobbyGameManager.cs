using Google.Protobuf;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using WebSocketSharp;
using UnityEngine.ResourceManagement.AsyncOperations;
using Unity.VisualScripting;

/*
 * login scene에서 전달 받은 token으로 player data 요청
 */
public class LobbyGameManager : MonoBehaviour
{
	UILobbySO _uiso;
	PlayerInfoSO _playerInfo;
	TCPClientSO _tcpClient;
	DateTime _lastRefreshTime = DateTime.MinValue;
	//bool _refreshing;

	Coroutine _taskCo;
	Coroutine _notifyCo;
	bool _playerDataReceived;

	void Awake()
	{
		if (FindAnyObjectByType<UILobbySOHolder>() == null)
		{
			var obj = new GameObject("[UI Lobby Holder]");
			obj.AddComponent<UILobbySOHolder>();
		}

		// lobby를 시작했을 때 로비생성에 관여하는 SO는 있으면 안 됨
		var lobbyInfoSO = FindAnyObjectByType<SessionParameterSOHolder>();
		Destroy(lobbyInfoSO);

		var nho = FindAnyObjectByType<NetworkEventHandler>();
		Destroy(nho);

		//if (FindAnyObjectByType<PlayerInfoSOHolder>() == null)
		//{
		//	var obj = new GameObject("[Player Info Holder]");
		//	obj.AddComponent<PlayerInfoSOHolder>();
		//	DontDestroyOnLoad(obj);
		//}
	}

	void Start()
	{
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.Notification = FindAnyObjectByType<UINotification>();
		_uiso.ClearEvent();

		if (_playerInfo == null
			|| _tcpClient == null
			|| _uiso == null)
		{
			//throw new Exception("Where is the SO holder in lobby scene");
			GLogger.LogWarning("Where is the SO holder in lobby scene");
			return;
		}

		_uiso.OnClickCreateLobby += OnClickCreateLobby;
		_uiso.OnClickRefresh += OnClickRefresh;
		_uiso.OnClickLobby += OnClickLobby;
		_uiso.OnClickExit += OnClickExit;

		_tcpClient.OnReceived += OnTCPDataReceived;

		_taskCo = StartCoroutine(WaitForLobbyReady());
	}

	/*
	 * 1. player data를 수신할 때까지 UI잠그고 대기
	 * 2. 로비 리스트 갱신
	 */
	IEnumerator WaitForLobbyReady()
	{
		/*
		 * Start에 호출되는 메서드이므로 다른 오브젝트 초기화를 위해 한 프레임 대기
		 * ui 초기화가 끝나지 않은 경우 있음
		 */
		yield return null;

		_uiso.SetInteractable(false);

		var t = RequestPlayerData();
		yield return new WaitUntil(() => t.IsCompleted);
		yield return new WaitUntil(() => _playerDataReceived);

		GLogger.Log("Received player data");

		_uiso.SetPlayerLabel(_playerInfo.Nickname, _playerInfo.PersonalColor);

		var t2 = UpdateLobbyList();
		yield return new WaitUntil(() => t2.IsCompleted);

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	async Task RequestPlayerData()
	{
		if (!UGSManager.IsInitialized())
		{
			await UGSManager.InitServices();
		}

		PMRequestPlayerData request = new()
		{
			Message = "hello"
		};

		var data = request.ToByteArray();
		await _tcpClient.SendDataAsync(
			(int)ProtoAuthenticationMessage.RequestPlayerData, data);
		GLogger.LogWarning("Request Player Data And Lobby");
	}

	/*
	 * 먼저 서버에서 ResponsePlayerData 응답을 받고 호출할 것
	 */
	async Task UpdateLobbyList()
	{
		if (!_playerDataReceived)
		{
			GLogger.LogWarning("LobbyGameManager.GetLobbyList 플레이어 데이터 수신 전");
			return;
		}

		var duration = DateTime.Now - _lastRefreshTime;

		if (duration.TotalMilliseconds <= 1000)
		{
			GLogger.Log($"LobbyGameManager.GetLobbyList Too many lobby update requests");
			return;
		}

		var list = await UGSLobbyManager.GetLobbyList();

		if (list != null)
		{
			GLogger.Log($"GetLobbyList lobby count: {list.Count}");

			if (list.Count == 0)
			{
				_uiso.ResizeLobbyList(0);
				return;
			}

			_uiso.ResizeLobbyList((uint)list.Count);

			var ls = new LobbySettings();
			for (int i = 0; i < list.Count; ++i)
			{
				var lobby = list[i];

				ls.Index = (uint)i;
				ls.Name = lobby.Name;
				ls.MaxPlayers = lobby.MaxPlayers;
				ls.AvailableSlots = lobby.AvailableSlots;
				ls.Id = lobby.Id;
				ls.IsPlaying = lobby.Data["Playing"].Value == "true" ? true : false;

				_uiso.SetLobbyInfoByIndex(ls);
			}

#if UNITY_EDITOR
			string text = "---Lobby List---\n";
			foreach (var lobby in list)
			{
				text += $"{lobby.Id} {lobby.Name} {lobby.AvailableSlots}/{lobby.MaxPlayers}\n";
			}
			text += "----------------\n";
			GLogger.Log(text);
#endif
		}
		else
		{
			_uiso.ResizeLobbyList(0);
		}

		_lastRefreshTime = DateTime.Now;
	}

	/*
	 * 네트워크 작업을 대기
	 * 완료할 때 까지 ui를 잠금
	 */
	IEnumerator LockInteractabilityUntilTaskComplete(Task task)
	{
		_uiso.SetInteractable(false);

		yield return new WaitUntil(() => task.IsCompleted);

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	IEnumerator LockInteractabilityUntilTaskComplete(IEnumerator co)
	{
		_uiso.SetInteractable(false);
		yield return co;
		_uiso.SetInteractable(true);
	}

	void OnClickRefresh()
	{
		if (_taskCo != null)
		{
			return;
		}

		_taskCo = StartCoroutine(LockInteractabilityUntilTaskComplete(UpdateLobbyList()));
	}

	void OnClickLobby(string lobbyId)
	{
		if (_taskCo != null)
		{
			GLogger.LogWarning("LobbyGameManager.OnClickLobby 다른 작업 처리 중");
			return;
		}

		_taskCo = StartCoroutine(AttemptToEnterLobby(lobbyId));

		return;
		////
		_playerInfo.LobbyIdForEntry = lobbyId;

		LoadScene("NGOTestScene");
	}

	void OnClickCreateLobby()
	{
		if (_taskCo != null)
		{
			GLogger.LogWarning("LobbyGameManager.OnClickCreateLobby 다른 작업이 진행 중");
			return;
		}

		_uiso.DialogManager.SetOnCancelDialog(() =>
		{
			_uiso.DialogManager.HideLobbyCreationDialog();
		});

		_uiso.DialogManager.ShowLobbyCreationDialog((lobbyName, lobbyPassword) =>
		{
			print($"OnCreateRoom {lobbyName} {lobbyPassword}");

			if (_taskCo != null)
			{
				GLogger.LogWarning("LobbyGameManager.OnClickCreateLobby");
				return;
			}

			_taskCo = StartCoroutine(CreateLobbyAsHostAndEnter(lobbyName, lobbyPassword));
		});
	}

	// client 자격으로 session 진입
	IEnumerator AttemptToEnterLobby(string lobbyId)
	{
		var taskJoinLobby = UGSLobbyManager.JoinLobbyById(lobbyId, null);
		yield return new WaitUntil(() => taskJoinLobby.IsCompleted);

		(var lobby, var reason ) = taskJoinLobby.Result;
		if (lobby == null)
		{
			if (reason == "lobbyFull")
			{
				ShowNotification("lobby-entry-error-full");
			}
			else
			{
				ShowNotification("lobby-entry-error-unknown");
			}

			yield break;
		}
		else
		{
			GLogger.Log("Successful Lobby Entry");

			var obj = new GameObject("[Session Parameter]");
			var gp = obj.AddComponent<SessionParameterSOHolder>().Data;
			DontDestroyOnLoad(obj);

			gp.LobbyId = lobbyId;
			gp.LobbyName = null;
			gp.LobbyPassword = null;
			gp.MaxPlayers = 4;

			LoadScene("SessionReadyScene");
		}
	}

	// host 자격으로 session 진입
	IEnumerator CreateLobbyAsHostAndEnter(string lobbyName, string lobbyPassword)
	{
		if (lobbyName == null)
		{
			GLogger.LogError("CreateLobbyAsHostAndEnter lobbyName is null");
			yield break;
		}

		if (lobbyName.Length < 1)
		{
			ShowNotification("lobby-error-name-<2");

			yield break;
		}

		var obj = new GameObject("[Session Parameter]");
		var gp = obj.AddComponent<SessionParameterSOHolder>().Data;
		DontDestroyOnLoad(obj);

		gp.LobbyId = null; 
		gp.LobbyName = lobbyName;
		gp.LobbyPassword = null;
		gp.MaxPlayers = 4;

		LoadScene("SessionReadyScene");
		_taskCo = null;
	}

	void OnClickExit()
	{
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
		}

		StartCoroutine(LockInteractabilityUntilTaskComplete(ExitFromLobby()));
	}

	IEnumerator ExitFromLobby()
	{
		PMRequestLogout message = new();
		var data = message.ToByteArray();
		var task = _tcpClient.SendDataAsync((int)ProtoAuthenticationMessage.RequestLogout, data);
		yield return new WaitUntil(() => task.IsCompleted);

		LoadScene("LoginScene");
	}

	async Task OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			await Awaitable.MainThreadAsync();
			LoadScene("LoginScene");

			return;
		}

		ProtoAuthenticationMessage type = (ProtoAuthenticationMessage)BitConverter.ToInt32(buffer, 4);
		Debug.Log($"LobbyGameManager.OnDataReceivecFromServer(type: {type}, len: {length})");

		if (type == ProtoAuthenticationMessage.ResponsePlayerData)
		{
			PMResponsePlayerData response;

			try
			{
				response = PMResponsePlayerData.Parser.ParseFrom(buffer, 12, length - 12);
			}
			catch (InvalidProtocolBufferException e)
			{
				GLogger.LogError($"ResonsePlayerData Message Parsing Exception!! {e.Message}");
				return;
			}

			if (response.Result)
			{
				_playerInfo.Nickname = response.Nickname;
				_playerInfo.PersonalColor = PlayerInfoSO.DeserializePersonalColor(response.PersonalColor);

				UGSLobbyManager.nickname = response.Nickname;
				UGSLobbyManager.PersonalColor = response.PersonalColor;
				_playerDataReceived = true;
			}
			else
			{
				await Awaitable.MainThreadAsync();
				GLogger.LogError($"Received ResponsePlayerData Error : {response.Message}");
				LoadScene("LoginScene");
			}
		}
	}

	void LoadScene(string sceneName)
	{
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);	
		}

		_uiso.ClearEvent();
		_tcpClient.OnReceived -= OnTCPDataReceived;
		SceneManager.LoadScene(sceneName);
	}

	void ShowNotification(string localizationKey)
	{
		if (_notifyCo != null)
		{
			StopCoroutine(_notifyCo);
		}

		_notifyCo = StartCoroutine(ShowNotificationCo(localizationKey));
	}

	IEnumerator ShowNotificationCo(string localizationKey)
	{
		var task =  LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			"DefaultStringTable", localizationKey, LocalizationSettings.SelectedLocale);
		yield return task;

		_uiso.Notification.ShowNotification(task.Result);

		_notifyCo = null;
	}
}
