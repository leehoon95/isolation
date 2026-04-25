using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "UIGameResultSO", menuName = "Scriptable Objects/UIGameResultSO")]
public class UIGameResultSO : ScriptableObject
{
	IUIGameResult _gameResult;
	IUICurtain _curtain;

	public IUIGameResult GameResult
	{
		get => _gameResult;
		set => _gameResult = value;
	}

	public IUICurtain Curtain
	{
		get => _curtain;
		set => _curtain = value;
	}

	public event UnityAction OnExitToLobby;

	public void ClearEvent()
	{
		OnExitToLobby = null;
	}

	public void RaiseOnExitToLobby()
		=> OnExitToLobby?.Invoke();
	public void ShowExitToLobbyButton()
		=> _gameResult.ShowExitToLobbyButton();
	public void SetGameResult(int result)
		=> _gameResult.SetResult(result);

	public void OpenCurtain()
		=> _curtain.Open();
}

public class UIGameResultSOHolder : SOHolderSinglton<UIGameResultSO, UIGameResultSOHolder>
{ }