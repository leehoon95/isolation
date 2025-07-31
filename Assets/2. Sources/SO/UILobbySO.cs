using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UILobbySO", menuName = "Scriptable Objects/UILobbySO")]
public class UILobbySO : ScriptableObject
{
    RoomList _roomList;

    public event Action<string> OnClickCreateRoom;
    public event Action OnClickSettings;
    public event Action OnClickExit;

	public void RaiseOnSelectedRoom()
    {
		OnClickCreateRoom?.Invoke("");
	}

    public void RaiseOnClickSettings()
    {
		OnClickSettings?.Invoke();
    }

    public void RaiseOnClickExit()
    {
        OnClickExit?.Invoke();

	}


}
