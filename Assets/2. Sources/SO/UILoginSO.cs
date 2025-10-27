using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[Serializable]
[CreateAssetMenu(fileName = "UILoginSO", menuName = "Scriptable Objects/UILoginSO")]
public class UILoginSO : ScriptableObject, ISupportNotificationUI
{
	ILoginUI _loginUI;
	INotificationUI _notification;
	ILoginDialogManager _dialogManager;

	public event Action<string> OnLoginEnter;

	// test
	public event Action OnDisconnect;
	public event Action OnSendUDPData;

	public ILoginUI LoginUI
	{
		get { return _loginUI; }
		set { _loginUI = value; }
	}

	public INotificationUI Notification
	{
		get { return _notification; }
		set { _notification = value; }
	}

	public ILoginDialogManager DialogManager
	{
		get { return _dialogManager; }
		set { _dialogManager = value; }
	}

	// UI Event service
	public void RaiseOnLoginEnter(string nickName) 
		=> OnLoginEnter?.Invoke(nickName);
	public void RaiseOnDisconnect() 
		=> OnDisconnect?.Invoke();
	public void RaiseOnSendUDPData() 
		=> OnSendUDPData?.Invoke();

	// Notice service
	public void ShowNotification(string text) 
		=> _notification?.ShowNotification(text);

	// Login Pannel
	public void SetNickname(string nickname) => _loginUI.SetNickname(nickname);
}

public class UILoginSOHolder : SOHolderSinglton<UILoginSO, UILoginSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}