using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UILoginSO", menuName = "Scriptable Objects/UILoginSO")]
public class UILoginSO : ScriptableObject
{
	UILoginPanel _loginPanel;
	UINotificationPanel _noticePanel;

	public event Action<string> OnLoginEnter;
	public event Action OnDisconnect;
	public event Action OnSendUDPData;

	// UI Event service
	public void RaiseOnLoginEnter(string nickName) => OnLoginEnter?.Invoke(nickName);

	public void RaiseOnDisconnect() => OnDisconnect?.Invoke();

	public void RaiseSendUDPData() => OnSendUDPData?.Invoke();

	// Notice service
	public void ShowNoticeOnTop(string text) => _noticePanel?.ShowNotice(text);

	// Login Pannel
	public void SetNickname(string nickname) => _loginPanel.SetNickname(nickname);
	// object setting method
	public void SetLoginPanelObject(UILoginPanel loginPanel) => _loginPanel = loginPanel;
	public void SetNoticePanelObejct(UINotificationPanel noticePanel) => _noticePanel = noticePanel;
}
