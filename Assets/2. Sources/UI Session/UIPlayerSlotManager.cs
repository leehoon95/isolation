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

	UISessionSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.PlayerSlotManager = this;
	}

	public void AddPlayer(ulong clientId, string playerName, bool host = false)
    {
		UIPlayerSlot slot = GetSlot(clientId);

		if (host)
		{
			slot.SetSlotText($"{clientId}_{playerName}\nHOST");
			slot.SetSlotStatus(2);
		}
		else
		{
			slot.SetSlotText($"{clientId}_{playerName}");
			slot.SetSlotStatus(1);
		}
	}

	public void RemovePlayer(ulong clientId)
	{
		UIPlayerSlot slot = GetSlot(clientId);
		slot.SetSlotText($"EMPTY");
		slot.SetSlotStatus(0);
	}

	UIPlayerSlot GetSlot(ulong clientId)
	{
		switch (clientId)
		{
			case 0:
				return _slot0;
			case 1:
				return _slot1;
			case 2:
				return _slot2;
			case 3:
				return _slot3;
			default:
				GLogger.LogWarning($"UIPlayerSlotManager.AddPlayer Invalid ClientId {clientId}");
				return null;
		}
	}
}
