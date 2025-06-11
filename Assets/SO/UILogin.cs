using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UILogin", menuName = "Scriptable Objects/UILogin")]
public class UILogin : ScriptableObject
{
	LoginPanel _loginPanel;
	NoticePanel _noticePanel;

	public event Action<string> OnLoginEnter;
	public event Action OnDisconnect;
	
	// UI Event service
	public void RaiseOnLoginEnter(string nickName)
	{
		OnLoginEnter?.Invoke(nickName);
	}

	public void RaiseOnDisconnect()
	{
		OnDisconnect?.Invoke();
	}
	
	// Notice service
	public void NoticeOnTop(string text)
	{
		_noticePanel?.ShowNotice(text);
	}

	// Login Pannel
	public void SetNickname(string nickname)
	{
		_loginPanel.SetNickname(nickname);
	}

	// object setting method
	public void SetLoginPanelObject(LoginPanel loginPanel)
	{
		_loginPanel = loginPanel;
	}

	public void SetNoticePanelObejct(NoticePanel noticePanel)
	{
		_noticePanel = noticePanel;
	}
}
