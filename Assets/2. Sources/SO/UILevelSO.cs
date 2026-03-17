using System;
using UnityEngine;

[CreateAssetMenu(fileName = "UILevelSO", menuName = "Scriptable Objects/UILevelSO")]
public class UILevelSO : ScriptableObject
{
	IUIItemPickerPanel _itemPickerPanel;

	public IUIItemPickerPanel ItemPicker
	{
		get => _itemPickerPanel;
		set => _itemPickerPanel = value;
	}

	public event Action<int> OnTestEvent;

    public void RaiseTestEvent(int index) => OnTestEvent?.Invoke(index);
	public void ShowItemPicker(Vector2 position, string itemEffect, bool frontMode)
		=>_itemPickerPanel.ShowItemPicker(position, itemEffect, frontMode);
	public void MoveItemPicket(Vector2 position) => _itemPickerPanel.MoveItemPicker(position);

	public void HideItemPicker()
	{
		_itemPickerPanel.HideItemPicker();
	}

	public bool IsShowingItemPicker() => _itemPickerPanel.IsShowingItemPicker();

	public int GetPickedItemsIndex()
	{
		return _itemPickerPanel.GetSelectedIndex();
	}
}

public class UILevelSOHolder : SOHolderSinglton<UILevelSO, UILevelSOHolder>
{}