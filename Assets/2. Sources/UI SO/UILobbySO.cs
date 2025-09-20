using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "UILobbySO", menuName = "Scriptable Objects/UILobbySO")]
public class UILobbySO : ScriptableObject, ISupportNotificationUI
{
	IRoomListUI _roomList;
	ICommunicationBoxUI _communicationBox;
	ILobbyDialogManager _dialogManager;
	INotificationUI _notification;

	public IRoomListUI RoomList
	{
		get => _roomList;
		set => _roomList = value;
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
	public event Action OnClickCreateRoom;
	public event Action OnClickSettings;
	public event Action OnClickRefresh;
	public event Action OnClickExit;
	public event Action<int> OnClickRoom;
	public event Action<string> OnSendMessage;
	public event Action OnCancelDialog;

	// Buttons
	public void RaiseOnClickRoom(int roomIndex) => OnClickRoom?.Invoke(roomIndex);
	public void RaiseOnClickCreateRoom() => OnClickCreateRoom?.Invoke();
	public void RaiseOnClickSettings() => OnClickSettings?.Invoke();
	public void RaiseOnClickRefresh() => OnClickRefresh?.Invoke();
	public void RaiseOnClickExit() => OnClickExit?.Invoke();
	public void RaiseOnEndEditMessage(string message) => OnSendMessage?.Invoke(message);

	// Dialog
	public void RaiseOnCancelDialog() => OnCancelDialog?.Invoke();

	// Notification
	public void ShowNotification(string content)
		=> _notification?.ShowNotification(content);
}
