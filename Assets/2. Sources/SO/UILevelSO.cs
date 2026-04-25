using System;
using System.Diagnostics.Contracts;
using UnityEngine;

[CreateAssetMenu(fileName = "UILevelSO", menuName = "Scriptable Objects/UILevelSO")]
public class UILevelSO : ScriptableObject
{
	INotificationUI _notification;
	IUIItemPickerPanel _itemPickerPanel;
	IUIStatusIndicator _statusIndicator;
	IUICurtain _curtain;

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

	public INotificationUI Notification
	{
		get => _notification;
		set => _notification = value;
	}

	public IUICurtain Curtain
	{
		get => _curtain;
		set => _curtain = value;
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

	public void OpenCurtain()
		=> _curtain.Open();
	public void CloseCurtain()
		=> _curtain.Close();

	public void ShowNotification(string text)
	=> _notification?.ShowNotification(text);
}

public class UILevelSOHolder : SOHolderSinglton<UILevelSO, UILevelSOHolder>
{}