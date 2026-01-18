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
	public void SetSlotData(int slotIndex, string playerName, Color color);
	public void SetReadyState(int slotIndex, bool ready);
	public void EmptySlot(int slotIndex);
	public void SetIsYou(int index);
	public void SetInteractable(bool interactable);
}

public interface IUISessionCommunication
{
	public void HideReadyButton();
	public void AddMessage(string speaker, string message, Color personalColor);
	public void SetInteractable(bool interactable);
}