using UnityEngine;
using UnityEngine.Events;

public class LobbySettings
{
	public uint Index;
	public string Name;
	public int MaxPlayers;
	public int AvailableSlots;
	public string Id;
	public bool IsPlaying;
}

public interface IUILobbyList
{
	public void ResizeLobbyList(uint size, bool detroy = false);
	public void SetLobby(LobbySettings settings); 
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