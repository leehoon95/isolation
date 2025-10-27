using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UINGOTestSO", menuName = "Scriptable Objects/UINGOTestSO")]
public class UINGOTestSO : ScriptableObject, ISupportNotificationUI
{
	INGOTestButtonUI _ngoTestButton;
	INGOTextUI _ngoText;
	INotificationUI _notification;

	public INGOTestButtonUI NGOTestButton
	{
		get { return _ngoTestButton; }
		set { _ngoTestButton = value; }
	}

	public INGOTextUI NGOText
	{
		get { return _ngoText; }
		set { _ngoText = value; }
	}

	public INotificationUI Notification
	{
		get => _notification;
		set => _notification = value;
	}

	public event Action OnClick_1;
	public event Action OnClick_2;
	public event Action OnClick_3;
	public event Action OnClick_4;
	public event Action OnClick_5;
	public event Action OnClick_6;

	public void Raise_1() => OnClick_1?.Invoke();
	public void Raise_2() => OnClick_2?.Invoke();
	public void Raise_3() => OnClick_3?.Invoke();
	public void Raise_4() => OnClick_4?.Invoke();
	public void Raise_5() => OnClick_5?.Invoke();
	public void Raise_6() => OnClick_6?.Invoke();
	public void ClearEvent()
	{
		OnClick_1 = null;
		OnClick_2 = null;
		OnClick_3 = null;
		OnClick_4 = null;
		OnClick_5 = null;
		OnClick_6 = null;
	}

	public void ShowText(string text) => _ngoText?.ShowText(text);

	public void ShowNotification(string content) 
		=> _notification?.ShowNotification(content);
}

public class UINGOTestSOHolder : SOHolderSinglton<UINGOTestSO, UINGOTestSOHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}