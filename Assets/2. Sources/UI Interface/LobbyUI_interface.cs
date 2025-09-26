using UnityEngine.Events;

public interface ISessionListUI
{
	public void ResizeSessionList(int minimumSession);
	public void SetSessionInfoIndex(
		int index,
		int sessionIndex,
		string name,
		int maxClientCount,
		int clientCount,
		string password,
		string joinCode);

#if UNITY_EDITOR
	public void AddTempSession();
#endif
}

public interface ICommunicationBoxUI
{

}

public interface ILobbyDialogManager
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