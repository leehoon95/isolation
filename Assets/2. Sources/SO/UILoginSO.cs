using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UILoginSO", menuName = "Scriptable Objects/UILoginSO")]
public class UILoginSO : ScriptableObject
{
	UILoginPanel _loginPanel;
	UINotificationPanel _noticePanel;
	UIDialog _dialog;

	public event Action<string> OnLoginEnter;
	public event Action OnDisconnect;
	public event Action OnSendUDPData;

	// UI Event service
	public void RaiseOnLoginEnter(string nickName) => OnLoginEnter?.Invoke(nickName);
	public void RaiseOnDisconnect() => OnDisconnect?.Invoke();
	public void RaiseSendUDPData() => OnSendUDPData?.Invoke();

	// Notice service
	public void SetNoticePanelObejct(UINotificationPanel noticePanel) => _noticePanel = noticePanel;
	public void ShowNoticeOnTop(string text) => _noticePanel?.ShowNotice(text);

	// Login Pannel
	public void SetLoginPanelObject(UILoginPanel loginPanel) => _loginPanel = loginPanel;
	public void SetNickname(string nickname) => _loginPanel.SetNickname(nickname);
	public void SetLoginButtonActive(bool active) => _loginPanel.SetLoginButtonActive(active);

	// Dialog
	public void SetDialogObejct(UIDialog dialog) => _dialog = dialog;
	public void ShowDialog(bool show) => _dialog.ShowDialog(show);
	public void SetDialogType(UIDialogType type) => _dialog.SetType(type);
	public void SetDialogTitleText(string text) => _dialog.SetTitle(text);
	public void SetDialogContentText(string text) => _dialog.SetContent(text);
	public void SetDialogOKButtonText(string text) => _dialog.SetOKButtonText(text);
	public void SetDialogOKButtonOnClickListner(UnityAction listner) 
		=> _dialog.SetOKButtonOnClickListner(listner);
	public void RemoveDialogOKButtonOnClickListner(UnityAction listner) 
		=> _dialog.RemoveOkButtonOnClickListner(listner);
}
