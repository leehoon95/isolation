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
	public void ShowItemPicker(Vector2 position)
	{
		_itemPickerPanel.ShowItemPicker(position);
	}

	public void HideItemPicker()
	{
		_itemPickerPanel.HideItemPicker();
	}
}

public class UILevelSOHolder : SOHolderSinglton<UILevelSO, UILevelSOHolder>
{}