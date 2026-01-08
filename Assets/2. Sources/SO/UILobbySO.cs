using System;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "UILobbySO", menuName = "Scriptable Objects/UILobbySO")]
public class UILobbySO : ScriptableObject
{
	IUILobbyList _lobbyList;
	IUILobbyPlayerLabel _playerLabel;
	IUILobbyButtons _buttons;
	IUILobbyPlayerList _playerList;
	IUILobbyDialogManager _dialogManager;
	INotificationUI _notification;

	public IUILobbyList LobbyList
	{
		get => _lobbyList;
		set => _lobbyList = value;
	}

	public IUILobbyPlayerLabel PlayerLabel
	{
		get => _playerLabel;
		set => _playerLabel = value;
	}

	public IUILobbyButtons Buttons
	{
		get => _buttons;
		set => _buttons = value;
	}

	public IUILobbyPlayerList PlayerList
	{
		get => _playerList;
		set => _playerList = value;
	}

	public IUILobbyDialogManager DialogManager
	{
		get => _dialogManager;
		set => _dialogManager = value;
	}

	public INotificationUI Notification
	{
		get => _notification;
		set => _notification = value;
	}

	// event
	public event Action OnClickCreateLobby;
	public event Action OnClickSettings;
	public event Action OnClickRefresh;
	public event Action OnClickExit;
	public event Action<string> OnClickLobby;

	public void ClearEvent()
	{
		OnClickSettings = null;
		OnClickRefresh = null;
		OnClickExit = null;
		OnClickLobby = null;
	}

	// Buttons
	public void RaiseOnClickSession(string lobbyId) => OnClickLobby?.Invoke(lobbyId);
	public void RaiseOnClickCreateLobby() => OnClickCreateLobby?.Invoke();
	public void RaiseOnClickSettings() => OnClickSettings?.Invoke();
	public void RaiseOnClickRefresh() => OnClickRefresh?.Invoke();
	public void RaiseOnClickExit() => OnClickExit?.Invoke();

	// Notification
	public void ShowNotification(string content)
		=> _notification?.ShowNotification(content);

	public void ResizeLobbyList(uint size = 0) => _lobbyList.ResizeLobbyList(size);
	public void SetLobbyInfoByIndex(
		uint index,
		string name,
		int maxPlayers,
		int currentPlayer,
		string lobbyId) 
		=> _lobbyList.SetLobbyInfoIndex(
			index, 
			name,
			maxPlayers,
			currentPlayer, 
			lobbyId);

	public void SetPlayerLabel(string nickname, Color personalColor)
		=> _playerLabel.SetPlayerLabel(nickname, personalColor);

	public void SetInteractable(bool interactable)
	{
		_lobbyList.SetInteractable(interactable);
		_buttons.SetInteractable(interactable);
	}
}

public class UILobbySOHolder : SOHolderSinglton<UILobbySO, UILobbySOHolder>
{
}