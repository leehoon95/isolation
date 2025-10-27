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
	public event Action<string> OnClickSession;
	public event Action<string> OnSendMessage;
	public event Action OnCancelDialog;

	// Buttons
	public void RaiseOnClickSession(string lobbyId) => OnClickSession?.Invoke(lobbyId);
	public void RaiseOnClickCreateSession() => OnClickCreateSession?.Invoke();
	public void RaiseOnClickSettings() => OnClickSettings?.Invoke();
	public void RaiseOnClickRefresh()
		=> OnClickRefresh?.Invoke();
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
		string name,
		int maxPlayerCount,
		int playerCount,
		string lobbyId) 
		=> _sessionList.SetSessionInfoIndex(
			index, 
			name, 
			maxPlayerCount, 
			playerCount, 
			lobbyId);
	public void ShowEmptySessionListNotification(bool show)
		=> _sessionList.ShowEmptySessionListNotification(show);

#if UNITY_EDITOR
	public void AddTempSession() => _sessionList.AddTempSession();
#endif
}
public class UILobbySOHolder : SOHolderSinglton<UILobbySO, UILobbySOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}