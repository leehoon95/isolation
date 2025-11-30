using UnityEngine;

[CreateAssetMenu(fileName = "UISessionSO", menuName = "Scriptable Objects/UISessionSO")]
public class UISessionSO : ScriptableObject, ISupportNotificationUI
{
	INotificationUI _notification;

	public INotificationUI Notification 
	{ 
		get => _notification; 
		set => _notification = value; 
	}

	public void ShowNotification(string text)
	{
		_notification?.ShowNotification(text);
	}
}

public class UISessionSOHolder : SOHolderSinglton<UISessionSO, UISessionSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}