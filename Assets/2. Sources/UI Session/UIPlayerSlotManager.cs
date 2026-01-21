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

	public void SetReadyState(int slotIndex, bool ready, bool isMe)
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
			if (isMe)
			{
				slot.SlotStatus = PlayerSlotStatus.InUseAndMe;
				slot.ThisIsMe = true;
			}
			else
			{
				slot.SlotStatus = PlayerSlotStatus.InUse;
				slot.ThisIsMe = false;
			}
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

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}
}
