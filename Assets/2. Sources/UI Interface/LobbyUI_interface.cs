using UnityEngine.Events;

public interface IUISessionList
{
	public void ResizeSessionList(int minimumSession);
	public void SetSessionInfoIndex(
		int index,
		string name,
		int maxPlayerCount,
		int playerCount,
		string lobbyId);
	public void ShowEmptySessionListNotification(bool show);

#if UNITY_EDITOR
	public void AddTempSession();
#endif
}

public interface IUICommunicationBox
{

}

public interface IUILobbyDialogManager
{
	// Common
	public void CloseDialog();

	// CR: Create Session
	public void OpenDialog_CR();
	public void SetTitle_CR(string title);
	public void SetContent_CR(string content);
	public void AddOnOk_CR(UnityAction<string, string> ua);
	public void RemoveOnOk_CR(UnityAction<string, string> ua);


}