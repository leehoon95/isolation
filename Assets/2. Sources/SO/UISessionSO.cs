using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UISessionSO", menuName = "Scriptable Objects/UISessionSO")]
public class UISessionSO : ScriptableObject
{
	INotificationUI _notification;
	IUIPlayerSlotManager _playerSlotManager;
	IUISessionCommunication _sessionCommunication;

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

	public IUISessionCommunication SessionCommunication
	{
		get => _sessionCommunication;
		set => _sessionCommunication = value;
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
	public void AddMessage(string speaker, string text, Color personalColor) => _sessionCommunication.AddMessage(speaker, text, personalColor);
	public void ClearEvent()
	{
		OnClickReady = null;
		OnClickLeave = null;
		OnSubmitMessage = null;
	}

	public void SetInteractable(bool interactable)
	{
		_playerSlotManager.SetInteractable(interactable);
		_sessionCommunication.SetInteractable(interactable);
	}
}

public class UISessionSOHolder : SOHolderSinglton<UISessionSO, UISessionSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}