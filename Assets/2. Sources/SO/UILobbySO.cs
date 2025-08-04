using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UILobbySO", menuName = "Scriptable Objects/UILobbySO")]
public class UILobbySO : ScriptableObject
{
    RoomList _roomList;
    CommunicationBox _communicationBox;

    public event Action<int> OnClickCreateRoom;
    public event Action OnClickSettings;
    public event Action OnClickRefresh;
    public event Action OnClickExit;
    public event Action<string> OnSendMessage;

    // UI
	public void RaiseOnSelectedRoom(int roomIndex) => OnClickCreateRoom?.Invoke(roomIndex);
	public void RaiseOnClickSettings() => OnClickSettings?.Invoke();
	public void RaiseOnClickRefresh() => OnClickRefresh?.Invoke();
	public void RaiseOnClickExit() => OnClickExit?.Invoke();
    public void RaiseOnEndEditMessage(string message) => OnSendMessage?.Invoke(message);
    public void SetRoomList(RoomList rl) => _roomList = rl;
    public void SetCommunicationBox(CommunicationBox cb) => _communicationBox = cb;

    // GM
    public void RefreshRommList(RM_ResponseRoomList rrl) => _ =_roomList.SetRoomItem(rrl);
}
