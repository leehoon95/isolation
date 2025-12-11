using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UISessionSO", menuName = "Scriptable Objects/UISessionSO")]
public class UISessionSO : ScriptableObject, ISupportNotificationUI
{
	INotificationUI _notification;
	IUIPlayerSlotManager _playerSlotManager;
	IUIMessageList _messageList;

	public INotificationUI Notification 
	{ 
		get => _notification; 
		set => _notification = value; 
	}

	public IUIPlayerSlotManager PlayerSlotManager
	{
		get => _playerSlotManager;
		set => _playerSlotManager = value;
	}

	public IUIMessageList MessageList
	{
		get => _messageList;
		set => _messageList = value;
	}

	public void ShowNotification(string text)
	{
		_notification?.ShowNotification(text);
	}

	// event
	public event Action OnClickReady;
	public event Action OnClickLeave;
	public event Action<string> OnSubmitMessage;

	public void RaiseOnClickReady() => OnClickReady?.Invoke();
	public void RaiseOnClickLeave() => OnClickLeave?.Invoke();
	public void RaiseOnSubmitMessage(string text) => OnSubmitMessage?.Invoke(text);
	public void AddMessage(string text, Color color) => _messageList.AddMessage(text, color);
}

public class UISessionSOHolder : SOHolderSinglton<UISessionSO, UISessionSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}