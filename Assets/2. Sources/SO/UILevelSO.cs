using System;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(fileName = "UILevelSO", menuName = "Scriptable Objects/UILevelSO")]
public class UILevelSO : ScriptableObject
{
	IUIItemPickerPanel _itemPickerPanel;
	IUIStatusIndicator _statusIndicator;

	public IUIItemPickerPanel ItemPicker
	{
		get => _itemPickerPanel;
		set => _itemPickerPanel = value;
	}

	public IUIStatusIndicator StatusIndicator
	{
		get => _statusIndicator;
		set => _statusIndicator = value;
	}

	public event Action<int> OnTestEvent;

    public void RaiseTestEvent(int index) 
		=> OnTestEvent?.Invoke(index);

	public void ShowItemPicker(Vector2 position, string itemEffect, bool frontMode)
		=>_itemPickerPanel.ShowItemPicker(position, itemEffect, frontMode);
	public void MoveItemPicket(Vector2 position) 
		=> _itemPickerPanel.MoveItemPicker(position);
	public void HideItemPicker()
		=> _itemPickerPanel.HideItemPicker();
	public bool IsShowingItemPicker() => _itemPickerPanel.IsShowingItemPicker();
	public int GetPickedItemsIndex()
		=> _itemPickerPanel.GetSelectedIndex();

	public void ShowIndicator(bool show)
		=> _statusIndicator.ShowIndicator(show);
	public void UpdateIndicator(int health, int sheild, string buff = "")
		=> _statusIndicator.UpdateIndicator(health, sheild, buff);
	public void UpdateIndicatorPosition(Vector2 position)
		=> _statusIndicator.UpdateIndicatorPosition(position);
}

public class UILevelSOHolder : SOHolderSinglton<UILevelSO, UILevelSOHolder>
{}