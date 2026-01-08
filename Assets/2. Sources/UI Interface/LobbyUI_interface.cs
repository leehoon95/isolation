using UnityEngine;
using UnityEngine.Events;

public interface IUILobbyList
{
	public void ResizeLobbyList(uint size, bool detroy = false);
	public void SetLobbyInfoIndex(
		uint index,
		string name,
		int maxPlayers,
		int currentPlayer,
		string lobbyId); 
	public void SetInteractable(bool interactable);
}

public interface IUILobbyButtons
{
	public void SetInteractable(bool interactable);
}

public interface IUILobbyPlayerList
{
	public void AddPlayer(string playerName, Color color);
	public void RemovePlayer(string playerName);
}

public interface IUILobbyDialogManager
{
	public void SetOnCancelDialog(UnityAction onCancel);

	public void ShowLobbyCreationDialog(UnityAction<string, string> onSubmit);
	public void HideLobbyCreationDialog();
	public void SetInteractable(bool interactable);
}

public interface IUILobbyPlayerLabel
{
	public void SetPlayerLabel(string nickname, Color personalColor);
}