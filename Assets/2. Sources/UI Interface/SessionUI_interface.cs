using UnityEngine;

public interface IUIPlayerSlotManager
{
	public void AddPlayer(ulong clieniId, string playerName, bool host = false);
	public void RemovePlayer(ulong clientId);
}

public interface IUIMessageList
{
	public void AddMessage(string message, Color color);
}