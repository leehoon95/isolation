using Google.Protobuf;
using System;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Rendering.LookDev;
using UnityEngine.SceneManagement;
using WebSocketSharp;

public class LoginGameManager : MonoBehaviour
{
	[SerializeField] 
	SaveDataLoader _sdl;
	TCPClientSO _tcpClient;
	UILoginSO _uiso;
	PlayerInfoSO _playerInfo;

	string _inputId;
	string _inputPassword;

	// 다수의 네트워크 작업을 동시에 진행하지 말 것
	Coroutine _taskCo;
	Coroutine _notifyCo;
	PMResponseRegisterAccount _responseRegisterAccount;

	void Awake()
	{
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

		if (FindAnyObjectByType<UILoginSOHolder>() == null)
		{
			var obj = new GameObject("[UI Login Holder]");
			obj.AddComponent<UILoginSOHolder>();
		}

		if (FindAnyObjectByType<UnobservedTaskExceptionHandlerHolder>() == null)
		{
			var obj = new GameObject("[Unobserved Task Exception Handler Holder]");
			var component = obj.AddComponent<UnobservedTaskExceptionHandlerHolder>();
			DontDestroyOnLoad(obj);

			/*
			 * Task에서 예외가 발생했으나 개발자가 명시적으로 처리하지 않은 예외를 런타임이 감지했을 때 발생하는 이벤트
			 * 작업이 GC되기 전, 즉 예외가 완전히 무시되기 전에 발생하며 로깅 또는 처리할 수 있는 기회 제공
			 * 핸들러가 없으면 app이 종료됨
			 */
			TaskScheduler.UnobservedTaskException
				+= component.Data.Handler;
		}
	}

	void Start()
	{
		_tcpClient = FindAnyObjectByType<TCPClientSOHolder>().Data;
		_playerInfo = FindAnyObjectByType<PlayerInfoSOHolder>().Data;
		_uiso = FindAnyObjectByType<UILoginSOHolder>().Data;
		_uiso.Notification = FindAnyObjectByType<UINotification>();
		_uiso.ClearEvent();
		_uiso.OnLogin += OnLogin;
		_uiso.OnRegister += OnRegister;
		//_uiso.OnTest_1 += OnDisconnect;
		_uiso.OnTest_2 += () => {
			if (_taskCo != null)
			{
				GLogger.LogWarning("OnRegister 다른 작업이 진행 중");
				return;
			}

			PMRequestRegisterAccount request = new()
			{
				Id = "qqq",
				Password = "d2",
				Nickname = "3d",
				PersonalColor = $"255/255/255"
			};

			var data = request.ToByteArray();

			_taskCo = StartCoroutine(
				LockInteractabilityUntilTaskComplete(
					_tcpClient.SendDataAsync(
						(int)ProtoAuthenticationMessage.RequestRegisterAccount, data)));
		};

		_tcpClient.OnReceived += OnTCPDataReceived;
		_taskCo = StartCoroutine(ReadyForLoginScene());
	}

	IEnumerator ReadyForLoginScene()
	{
		yield return LocalizationSettings.InitializationOperation;
		//var op = LocalizationSettings.StringDatabase.PreloadOperation;
		//while (!op.IsDone)
		//{
		//	yield return new WaitForSeconds(0.02f);
		//}
		var t = LocalDataSettings.Instance.LoadAsync();
		yield return new WaitUntil(() => t.IsCompleted);

		var data = LocalDataSettings.Instance.Data;

		if (data.Id.Length >= 2 && data.Password.Length >= 2)
		{
			_uiso.LoginUI.SetId(data.Id);
			_uiso.LoginUI.SetPassword(data.Password);
		}

		yield return StartCoroutine(ConnectToServerCoroutine());
	}

	IEnumerator ConnectToServerCoroutine()
	{
		yield return null;

		_uiso.SetInteractable(false);
		var task = ConnectToServer();
		yield return new WaitUntil(() => task.IsCompleted);

		if (!task.Result)
		{
			OpenNetworkErrorDialog();
		}
		else
		{
			GLogger.Log("Connected to server");
		}

		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	async Task<bool> ConnectToServer()
	{
		if (_tcpClient.Connnected)
		{
			Debug.Log("Server is connected already");
			return true;
		}

		return await _tcpClient.ConnectToServer();
	}

	void OpenNetworkErrorDialog()
	{
		GLogger.Log("OpenNetworkErrorDialog");
		var selectedLocale = LocalizationSettings.SelectedLocale;
		var adb = LocalizationSettings.StringDatabase;
		_uiso.DialogManager.ShowOkDialog(
			adb.GetLocalizedString("DefaultStringTable", "network-connection-error", selectedLocale),
			adb.GetLocalizedString("DefaultStringTable", "network-connection-error-massage", selectedLocale),
			adb.GetLocalizedString("DefaultStringTable", "retry", selectedLocale),
				() =>
				{
					if (_taskCo != null)
					{
						GLogger.LogWarning("OpenNetworkErrorDialog 다른 작업이 진행 중");
						return;
					}

					_uiso.DialogManager.HideOkDialog();
					_taskCo = StartCoroutine(ConnectToServerCoroutine());
				}
			);
		_uiso.DialogManager.SetOnCancelDialog(() => _uiso.DialogManager.HideOkDialog());
	}

	void OnLogin(string id, string password)
	{
		if (_taskCo != null)
		{
			GLogger.LogWarning("OnLogin 다른 작업이 진행 중");
			return;
		}

		if (!_tcpClient.Connnected)
		{
			GLogger.LogWarning("OnLogin Not connected from server");

			_taskCo = StartCoroutine(ConnectToServerCoroutine());
		}

		if (id == null || id.Length < 2)
		{
			return;
		}

		if (password == null || password.Length < 2)
		{
			return;
		}

		PMRequestLogin msg = new();
		msg.Id = _inputId = id;
		msg.Password = _inputPassword = password;

		var data = msg.ToByteArray();

		_taskCo = StartCoroutine(
			LockInteractabilityUntilTaskComplete(
				_tcpClient.SendDataAsync(
					(int)ProtoAuthenticationMessage.RequestLogin, data)));
	}

	void OnRegister()
	{
		if (_taskCo != null)
		{
			ShowNotification("another-task-in-progress");
			return;
		}

		if (!_tcpClient.Connnected)
		{
			ShowNotification("retry-connect-server");

			_taskCo = StartCoroutine(ConnectToServerCoroutine());
			return;
		}

		_uiso.DialogManager.SetOnCancelDialog(() =>
		{
			if (_taskCo != null)
			{
				ShowNotification("another-task-in-progress");
				return;
			}

			_uiso.DialogManager.HideAccountCreationDialog();
		});

		_uiso.DialogManager.ShowAccountCreationDialog(
			(application) =>
			{
				if (_taskCo != null)
				{
					ShowNotification("another-task-in-progress");
					return;
				}

				PMRequestRegisterAccount request = new()
				{
					Id = application.id,
					Password = application.password,
					Nickname = application.nickname,
					PersonalColor = $"{application.h}/{application.s}/{application.v}"
				};

				var data = request.ToByteArray();

				_responseRegisterAccount = null;
				_taskCo = StartCoroutine(
					WaitForAccountCreationResponse(
						_tcpClient.SendDataAsync(
							(int)ProtoAuthenticationMessage.RequestRegisterAccount, data)));
			});
	}

	IEnumerator WaitForAccountCreationResponse(Task dataTransferTask)
	{
		_uiso.DialogManager.SetAccountCreationDialogOkButtonWaiting(true);
		_uiso.SetInteractable(false);
		yield return new WaitUntil(() => dataTransferTask.IsCompleted);
		yield return new WaitUntil(() => _responseRegisterAccount != null);
		yield return HandleAccountCreationResultCo(_responseRegisterAccount);
		_uiso.SetInteractable(true);
		_responseRegisterAccount = null;
		_taskCo = null;
	}

	/*
	 * 네트워크 작업을 대기
	 * 완료할 때 까지 ui를 잠금
	 */
	IEnumerator LockInteractabilityUntilTaskComplete(Task task)
	{
		_uiso.DialogManager.HideAccountCreationDialog();
		_uiso.SetInteractable(false);
		yield return new WaitUntil(() => task.IsCompleted);
		_uiso.SetInteractable(true);
		_taskCo = null;
	}

	// test button
	void OnDisconnect()
	{
		//_ns.CloseConnection();
		_tcpClient.CloseConnection();
	}

	async Task OnTCPDataReceived(byte[] buffer, int length)
	{
		if (length == 0)
		{
			await Awaitable.MainThreadAsync();

			_uiso.ShowNotification(
				LocalizationSettings.StringDatabase.GetLocalizedString(
					"DefaultStringTable",
					"network-disconnected-from-server",
					LocalizationSettings.SelectedLocale));

			_uiso.DialogManager.HideAccountCreationDialog();
			_uiso.DialogManager.HideYesNoDialog();
			_uiso.DialogManager.HideOkDialog();

			if (_taskCo != null)
			{
				GLogger.LogError("LoginGameManager.OnTCPDataReceived Disconnected from server. 다른 네트워크 작업 진행 중");
				return;
			}

			_taskCo = StartCoroutine(ConnectToServerCoroutine());

			return;
		}

		ProtoAuthenticationMessage type = (ProtoAuthenticationMessage)BitConverter.ToInt32(buffer, 4);
		GLogger.Log($"LoginGameManager.OnDataReceivecFromServer(type: {type}, len: {length})");

		if (type == ProtoAuthenticationMessage.ResponseLogin)
		{
			PMResponseLogin msg;

			try
			{
				msg = PMResponseLogin.Parser.ParseFrom(buffer, 12, length - 12);
				if (msg == null)
				{
					GLogger.LogError($"LoginGameManager.OnTCPDataReceived Parsing error {type}");
					return;
				}
			}
			catch (InvalidProtocolBufferException e)
			{
				GLogger.LogException(e, this);
				return;
			}

			if (msg.Result)
			{
				GLogger.Log("LOGIN SUCCESS");

				var data = LocalDataSettings.Instance.Data;
				data.Id = _inputId;
				data.Password = _inputPassword;
				
				await LocalDataSettings.Instance.SaveAsync();

				await Awaitable.MainThreadAsync();

				_playerInfo.Token = msg.Token;
				LoadScene("LobbyScene");
			}
			else
			{
				GLogger.LogError($"Login request is Denied. (reason: {msg.Message})");
			}
		}
		else if (type == ProtoAuthenticationMessage.ResponseRegisterAccount)
		{
			PMResponseRegisterAccount msg;

			try
			{
				msg = PMResponseRegisterAccount.Parser.ParseFrom(buffer, 12, length - 12);
				if (msg == null)
				{
					GLogger.LogError($"LoginGameManager.OnTCPDataReceived Parsing error {type}");
					return;
				}
			}
			catch (InvalidProtocolBufferException e)
			{
				GLogger.LogException(e, this);
				return;
			}

			_responseRegisterAccount = msg;
			//await Awaitable.MainThreadAsync();
			//StartCoroutine(ShowAccountCreationResultCo(msg));
		}
	}

	IEnumerator HandleAccountCreationResultCo(PMResponseRegisterAccount msg)
	{
		var selectedLocale = LocalizationSettings.SelectedLocale;
		var adb = LocalizationSettings.StringDatabase;
		string notify;
		string reason = "";

		if (msg.Result)
		{
			/*
			 * Async 함수임에도 내부에서 get_isPlaying 사용으로 인해 메인 스레드에서 호출되어야 한다
			 */
			var op = adb.GetLocalizedStringAsync(
				"DefaultStringTable", "account-creation-successful", selectedLocale);
			yield return op;
			notify = op.Result;

			_uiso.DialogManager.HideAccountCreationDialog();
		}
		else
		{
			GLogger.LogWarning($"Faield to create new account! {msg.Message}");
			var op = adb.GetLocalizedStringAsync(
				"DefaultStringTable", "account-creation-failed", selectedLocale);
			yield return op;

			notify = op.Result;
			string key = "account-creation-failure-reason-unknown";

			switch (msg.Message)
			{
				case "idLength<2":
					key = "account-creation-failure-reason-id-length-<2";
					break;
				case "passwordLength<2":
					key = "account-creation-failure-reason-password-length-<2";
					break;
				case "nicknameLength<2":
					key = "account-creation-failure-reason-nickname-length-<2";
					break;
				case "idAleadyInUse":
					key = "account-creation-failure-reason-id-aleady-in-use";
					break;
				default:
					key = "account-creation-failure-reason-unknown";
					break;
			}

			op = adb.GetLocalizedStringAsync(
						"DefaultStringTable", key, selectedLocale);
			yield return op;
			reason = op.Result;
		}

		_uiso.DialogManager.SetAccountCreationDialogOkButtonWaiting(false);
		_uiso.Notification.ShowNotification($"{notify}{(reason.Length > 0 ? '\n' : "")}{reason}");
	}

	void LoadScene(string sceneName)
	{
		_uiso.ClearEvent();
		_tcpClient.OnReceived -= OnTCPDataReceived;
		SceneManager.LoadScene(sceneName);
	}

	// 메인 스레드에서 호출할 것
	void ShowNotification(string localizationKey)
	{
		//if (_notifyCo != null)
		//{
		//	StopCoroutine(_notifyCo);
		//}

		//_notifyCo =
			StartCoroutine(ShowNotificationCo(localizationKey));
	}

	IEnumerator ShowNotificationCo(string localizationKey)
	{
		var task = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			"DefaultStringTable", localizationKey, LocalizationSettings.SelectedLocale);
		yield return task;

		_uiso.Notification.ShowNotification(task.Result);

		_notifyCo = null;
	}
}
