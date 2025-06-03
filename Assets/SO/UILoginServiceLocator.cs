using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UILoginServiceLocator", menuName = "Scriptable Objects/UILoginServiceLocator")]
public class UILoginServiceLocatorSO : ScriptableObject
{
	LoginPanel _loginPanel;
	NoticePanel _noticePanel;
	SaveDataLoader _sdl;

	public event UnityAction<string> OnLoginEnter;

	// UI Event service
	public void RaiseOnLoginEnter(string nickName)
	{
		OnLoginEnter?.Invoke(nickName);
	}

	// Notice service
	public void NoticeOnTop(string text)
	{
		_noticePanel?.ShowNotice(text);
	}

	public void SetLoginPanelObject(LoginPanel loginPanel)
	{
		_loginPanel = loginPanel;
	}

	public void SetNoticePanelObejct(NoticePanel noticePanel)
	{
		_noticePanel = noticePanel;
	}

	public void SetSaveDataLoader(SaveDataLoader sdl)
	{
		_sdl = sdl;
	}
}
