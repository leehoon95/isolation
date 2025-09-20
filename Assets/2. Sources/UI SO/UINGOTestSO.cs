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

	public event Action OnClickStartHost;
	public event Action OnClickStartClient;
	public event Action OnClickSpawn;
	public event Action OnClickShutdown;
	public event Action OnClickShowStatus;

	public void RaiseOnClickStartHost() => OnClickStartHost?.Invoke();
	public void RaiseOnClickStartClient() => OnClickStartClient?.Invoke();
	public void RaiseOnClickSpawn() => OnClickSpawn?.Invoke();
	public void RaiseOnClickShutdown() => OnClickShutdown?.Invoke();
	public void RaiseOnClickShowStatus() => OnClickShowStatus?.Invoke();

	public void SetText(string text) => _ngoText.SetText(text);

	public void ShowNotification(string content) 
		=> _notification?.ShowNotification(content);
}
