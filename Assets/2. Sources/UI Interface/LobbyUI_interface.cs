using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public interface IRoomListUI
{
    public void SetRoomList(M_ResponseSessionList rrl);
}

public interface ICommunicationBoxUI
{

}

public interface ILobbyDialogManager
{
	// Common
	public void CloseDialog();

	// CR: Create Room
	public void OpenDialog_CR();
	public void SetTitle_CR(string title);
	public void SetContent_CR(string content);
	public void AddOnOk_CR(UnityAction<string, string> ua);
	public void RemoveOnOk_CR(UnityAction<string, string> ua);


}