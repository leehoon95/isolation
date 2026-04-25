using UnityEngine;

public enum PlayerSlotStatus
{
	Empty,
	InUse,
	InUseAndMe,
	Ready,
	Host
}

public interface IUIPlayerSlotManager
{
	public void SetSlotData(int slotIndex, string playerName, Color color);
	public void SetReadyState(int slotIndex, bool ready, bool isMe);
	public void EmptySlot(int slotIndex);
	public void SetInteractable(bool interactable);
}

public interface IUISessionCommunication
{
	public void AddMessage(string speaker, string message, Color personalColor);
	public void SetReadyButtonText(string text);
	public void SetReadyButtonHighlight(bool bright);
	public void SetInteractable(bool interactable);
}