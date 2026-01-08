using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

[Serializable]
[CreateAssetMenu(fileName = "UILoginSO", menuName = "Scriptable Objects/UILoginSO")]
public class UILoginSO : ScriptableObject
{
	IUILoginPannel _loginUI;
	INotificationUI _notification;
	IUILoginDialogManager _dialogManager;

	public IUILoginPannel LoginUI
	{
		get { return _loginUI; }
		set { _loginUI = value; }
	}

	public INotificationUI Notification
	{
		get { return _notification; }
		set { _notification = value; }
	}

	public IUILoginDialogManager DialogManager
	{
		get { return _dialogManager; }
		set { _dialogManager = value; }
	}

	public event Action<string, string> OnLogin;
	public event Action OnRegister;

	// test
	public event Action OnTest_1;
	public event Action OnTest_2;

	public void ClearEvent()
	{
		OnLogin = null;
		OnRegister = null;
	}

	// UI Event service
	public void RaiseOnLogin(string nickName, string password) 
		=> OnLogin?.Invoke(nickName, password);
	public void RaiseOnRegister()
		=> OnRegister?.Invoke();
	public void RaiseTestEvent_1() 
		=> OnTest_1?.Invoke();
	public void RaiseTestEvent_2() 
		=> OnTest_2?.Invoke();

	// Notice service
	public void ShowNotification(string text) 
		=> _notification?.ShowNotification(text);
	
	// Login Pannel
	public void SetId(string id) => _loginUI.SetId(id);
	public void SetPassword(string password) => _loginUI.SetPassword(password);
	public void SetInteractable(bool interactable)
	{
		_loginUI.SetInteractable(interactable);
		_dialogManager.SetInteractable(interactable);
	}
}

public class UILoginSOHolder : SOHolderSinglton<UILoginSO, UILoginSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}