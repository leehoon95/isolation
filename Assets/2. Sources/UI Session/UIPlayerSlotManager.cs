using Codice.Client.BaseCommands;
using Mono.Cecil;
using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(RectTransform))]
public class UIPlayerSlotManager : UIBehaviour, IUIPlayerSlotManager
{
	[SerializeField]
	UIPlayerSlot[] _slots;
	[SerializeField]
	CanvasGroup _canvasGroup;

	UISessionSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.PlayerSlotManager = this;

		_slots[0].SlotStatus = PlayerSlotStatus.Host;
		for (int i = 1; i < _slots.Length; i++)
		{
			_slots[i].SlotStatus = PlayerSlotStatus.Empty;
		}
	}

	public void SetSlotData(int slotIndex, string nickname, Color color)
	{
		if (slotIndex < 0 || slotIndex > 3)
		{
			GLogger.LogWarning($"UIPlayerSlotManager.SetSlot Invalid slot index {slotIndex}");
			return;
		}

		var slot = _slots[slotIndex];
		slot.SlotText = nickname;
		slot.SlotTextColor = color;
	}

	public void SetReadyState(int slotIndex, bool ready)
	{
		if (slotIndex == 0)
		{
			return;
		}

		var slot = _slots[slotIndex];
		if (ready)
		{
			slot.SlotStatus = PlayerSlotStatus.Ready;
		}
		else
		{
			slot.SlotStatus = PlayerSlotStatus.InUse;
		}
	}

	public void EmptySlot(int slotIndex)
	{
		if (slotIndex == 0 || slotIndex > 3)
		{
			GLogger.LogWarning($"UIPlayerSlotManager.EmptySlot Invalid slot index {slotIndex}");
			return;
		}

		UIPlayerSlot slot = _slots[slotIndex];
		slot.SlotText = "";
		slot.SlotStatus = PlayerSlotStatus.Empty;
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

	public void SetIsYou(int index)
	{
		for (int i = 1; i < _slots.Length; i++)
		{
			if (i == index)
			{
				_slots[i].You = true;
			}
			else
			{
				_slots[i].You = false;
			}
		}
	}
}
