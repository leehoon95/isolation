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
	public void ShowItemPicker(Vector2 position, string itemEffect, bool onlyFront)
	{
		_itemPickerPanel.ShowItemPicker(position, itemEffect, onlyFront);
	}

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