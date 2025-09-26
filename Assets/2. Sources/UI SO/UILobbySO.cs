using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "UILobbySO", menuName = "Scriptable Objects/UILobbySO")]
public class UILobbySO : ScriptableObject, ISupportNotificationUI
{
	ISessionListUI _sessionList;
	ICommunicationBoxUI _communicationBox;
	ILobbyDialogManager _dialogManager;
	INotificationUI _notification;

	public ISessionListUI SessionList
	{
		get => _sessionList;
		set => _sessionList = value;
	}
	public ICommunicationBoxUI CommunicationBox
	{
		get => _communicationBox;
		set => _communicationBox = value;
	}
	public ILobbyDialogManager DialogManager
	{
		get => _dialogManager;
		set => _dialogManager = value;
	}
	public INotificationUI Notification
	{
		get => _notification;
		set => _notification = value;
	}

	// event
	public event Action OnClickCreateSession;
	public event Action OnClickSettings;
	public event Action OnClickRefresh;
	public event Action OnClickExit;
	public event Action<int> OnClickSession;
	public event Action<string> OnSendMessage;
	public event Action OnCancelDialog;

	// Buttons
	public void RaiseOnClickSession(int sessionIndex) => OnClickSession?.Invoke(sessionIndex);
	public void RaiseOnClickCreateSession() => OnClickCreateSession?.Invoke();
	public void RaiseOnClickSettings() => OnClickSettings?.Invoke();
	public void RaiseOnClickRefresh() => OnClickRefresh?.Invoke();
	public void RaiseOnClickExit() => OnClickExit?.Invoke();
	public void RaiseOnEndEditMessage(string message) => OnSendMessage?.Invoke(message);

	// Dialog
	public void RaiseOnCancelDialog() => OnCancelDialog?.Invoke();

	// Notification
	public void ShowNotification(string content)
		=> _notification?.ShowNotification(content);

	// Session List
	public void ResizeSessionList(int minimumSession = 0) => _sessionList.ResizeSessionList(minimumSession);
	public void SetSessionInfoIndex(
		int index,
		int sessionIndex,
		string name,
		int maxClientCount,
		int clientCount,
		string password,
		string joinCode) => _sessionList.SetSessionInfoIndex(index, sessionIndex, name, maxClientCount, clientCount, password, joinCode);
#if UNITY_EDITOR
	public void AddTempSession() => _sessionList.AddTempSession();
#endif
}
