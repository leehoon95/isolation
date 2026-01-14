using UnityEngine;

public enum PlayerSlotStatus
{
	Empty,
	InUse,
	Ready,
	Host
}

public interface IUIPlayerSlotManager
{
	public void SetPlayer(uint slotIndex, string playerName, Color color, bool host = false);
	public void ReadyPlayer(uint slotIndex, bool ready);
	public void RemovePlayer(uint clientId);
	public void SetInteractable(bool interactable);
}

public interface IUISessionCommunication
{
	public void AddMessage(string speaker, string message, Color personalColor);
	public void SetInteractable(bool interactable);
}