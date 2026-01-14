using Mono.Cecil;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(RectTransform))]
public class UIPlayerSlotManager : UIBehaviour, IUIPlayerSlotManager
{
	[SerializeField]
	UIPlayerSlot _slot0;
	[SerializeField]
	UIPlayerSlot _slot1;
	[SerializeField]
	UIPlayerSlot _slot2;
	[SerializeField]
	UIPlayerSlot _slot3;
	[SerializeField]
	CanvasGroup _canvasGroup;

	UISessionSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.PlayerSlotManager = this;
		_slot0.SlotStatus = PlayerSlotStatus.Empty;
		_slot1.SlotStatus = PlayerSlotStatus.Empty;
		_slot2.SlotStatus = PlayerSlotStatus.Empty;
		_slot3.SlotStatus = PlayerSlotStatus.Empty;
	}

	public void SetPlayer(uint slotIndex, string playerName, Color color, bool host = false)
    {
		UIPlayerSlot slot = GetSlot(slotIndex);
		slot.SlotText = $"{playerName}";

		if (host)
		{
			slot.SlotStatus = PlayerSlotStatus.Host;
		}
		else
		{
			
			slot.SlotStatus = PlayerSlotStatus.InUse;
		}
	}

	public void ReadyPlayer(uint slotIndex, bool ready)
	{
		UIPlayerSlot slot = GetSlot(slotIndex);

		if (ready)
		{
			slot.SlotStatus = PlayerSlotStatus.Ready;
		}
		else
		{
			slot.SlotStatus = PlayerSlotStatus.InUse;
		}
	}

	public void RemovePlayer(uint index)
	{
		UIPlayerSlot slot = GetSlot(index);
		slot.SlotText = "";
		slot.SlotStatus = PlayerSlotStatus.Empty;
	}

	

	UIPlayerSlot GetSlot(uint index)
	{
		switch (index)
		{
			case 0: return _slot0;
			case 1: return _slot1;
			case 2: return _slot2;
			case 3: return _slot3;
			default: return null;
		}
	}

	//UIPlayerSlot GetEmptySlot()
	//{
	//	if (_slot0.SlotStatus == PlayerSlotStatus.Empty)
	//	{
	//		return _slot0;
	//	}
	//	else if (_slot1.SlotStatus == PlayerSlotStatus.Empty)
	//	{
	//		return _slot1;
	//	}
	//	else if (_slot2.SlotStatus == PlayerSlotStatus.Empty)
	//	{
	//		return _slot2;
	//	}
	//	else if (_slot3.SlotStatus == PlayerSlotStatus.Empty)
	//	{
	//		return _slot3;
	//	}

	//	return null;
	//}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}
}
