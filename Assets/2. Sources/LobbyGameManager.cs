using Google.Protobuf;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using WebSocketSharp;

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

	bool _isTasking;
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
		if (lobbyInfoSO != null)
		{
			Destroy(lobbyInfoSO);
		}

		var neh = FindAnyObjectByType<NetworkEventHandler>();
		if (neh != null)
		{
			Destroy(neh);
		}
	}

	async void Start()
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

		if (_playerInfo.IsGuestLogin)
		{
			await EnterLobbyAsGuest();
		}
		else
		{
			await WaitForLobbyReady();
		}
	}

	/*
	 * 1. player data를 수신할 때까지 UI잠그고 대기
	 * 2. 로비 리스트 갱신
	 */
	async Awaitable WaitForLobbyReady()
	{
		await Task.Yield();
		GLogger.Log("WaitForLobbyReady");
		_uiso.SetInteractable(false);

		if (!UGSManager.IsInitialized())
		{
			await UGSManager.InitServices();
		}

		await RequestPlayerData();

		int receiveWaitingCount = 0;
		while (!_playerDataReceived)
		{
			await Task.Delay(50);
			receiveWaitingCount++;
			if (receiveWaitingCount == 60)
			{
				await SendLogoutMessage();

				LoadScene("LoginScene");
				return;
			}
		}

		GLogger.Log($"Received player data {_playerInfo.Nickname} {_playerInfo.PersonalColor}");

		_uiso.SetPlayerLabel(_playerInfo.Nickname, _playerInfo.PersonalColor);

		await UpdateLobbyList();

		_uiso.SetInteractable(true);
	}

	async Awaitable EnterLobbyAsGuest()
	{
		await Task.Yield();

		if (!UGSManager.IsInitialized())
		{
			await UGSManager.InitServices();
		}

		_uiso.SetInteractable(false);

		int h = UnityEngine.Random.Range(0, 255);
		int s = UnityEngine.Random.Range(192, 255);
		_playerInfo.Nickname = $"Guest_{UnityEngine.Random.Range(0, 9999):D5}";
		_playerInfo.PersonalColor = Color.HSVToRGB(
			h / 255f, s / 255f, 1f);

		UGSLobbyManager.nickname = _playerInfo.Nickname;
		UGSLobbyManager.PersonalColor = $"{h}/{s}/255";
		_playerDataReceived = true;

		GLogger.Log($"Enter lobby as guest {UGSLobbyManager.nickname} {UGSLobbyManager.PersonalColor}");

		_uiso.SetPlayerLabel(_playerInfo.Nickname, _playerInfo.PersonalColor);

		await UpdateLobbyList();
		//yield return new WaitUntil(() => t2.IsCompleted);

		_uiso.SetInteractable(true);
	}

	async Awaitable RequestPlayerData()
	{
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
	async Awaitable UpdateLobbyList()
	{
		if (!_playerDataReceived)
		{
			GLogger.LogWarning("플레이어 프로필을 수신하지 못 함");
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
			//GLogger.Log($"GetLobbyList lobby count: {list.Count}");

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

	async void OnClickRefresh()
	{
		if (_isTasking)
		{
			return;
		}

		_isTasking = true;
		_uiso.SetInteractable(false);

		await UpdateLobbyList();

		_isTasking = false;
		_uiso.SetInteractable(true);
	}

	async void OnClickLobby(string lobbyId)
	{
		if (_isTasking)
		{
			return;
		}

		_isTasking = true;
		_uiso.SetInteractable(false);

		await AttemptToEnterLobby(lobbyId);
		
		_isTasking = false;
		_uiso.SetInteractable(true);

		return;
	}

	void OnClickCreateLobby()
	{
		if (_isTasking)
		{
			GLogger.LogWarning("LobbyGameManager.OnClickCreateLobby 다른 작업이 진행 중");
			return;
		}

		_uiso.DialogManager.SetOnCancelDialog(() =>
		{
			_uiso.DialogManager.HideLobbyCreationDialog();
		});

		_uiso.DialogManager.ShowLobbyCreationDialog(
			$"{_playerInfo.Nickname} {DateTime.Now.ToString()}",
			(lobbyName, lobbyPassword) =>
		{
			if (_isTasking)
			{
				GLogger.LogWarning("LobbyCreationDialog 다른 작업이 진행 중");
				return;
			}

			CreateLobbyAndEnter(lobbyName, lobbyPassword);
		});
	}

	async Awaitable AttemptToEnterLobby(string lobbyId)
	{
		_isTasking = true;

		(var lobby, var reason) = await UGSLobbyManager.JoinLobbyById(lobbyId, null);
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

		_isTasking = false;
	}


	// host 자격으로 session 진입
	void CreateLobbyAndEnter(string lobbyName, string lobbyPassword)
	{
		if (lobbyName == null)
		{
			GLogger.LogError("CreateLobbyAndEnter lobbyName is null");
			return;
		}

		if (lobbyName.Length < 1)
		{
			ShowNotification("lobby-error-name-<2");

			return;
		}

		var obj = new GameObject("[Session Parameter]");
		var gp = obj.AddComponent<SessionParameterSOHolder>().Data;
		DontDestroyOnLoad(obj);

		gp.LobbyId = null; 
		gp.LobbyName = lobbyName;
		gp.LobbyPassword = null;
		gp.MaxPlayers = 4;

		LoadScene("SessionReadyScene");
	}

	async void OnClickExit()
	{
		if (_isTasking)
		{
			return;
		}

		_isTasking = true;
		_uiso.SetInteractable(false);

		if (!_playerInfo.IsGuestLogin)
		{
			await SendLogoutMessage();
		}

		LoadScene("LoginScene");
	}

	async Awaitable SendLogoutMessage()
	{
		PMRequestLogout message = new();
		var data = message.ToByteArray();
		await _tcpClient.SendDataAsync((int)ProtoAuthenticationMessage.RequestLogout, data);
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
