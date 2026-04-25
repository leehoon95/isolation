using UnityEngine;

public interface IUIItemPickerPanel
{
	public void ShowItemPicker(Vector2 position, string itemEffect, bool onlyFront = false);
	public void HideItemPicker();
	public void MoveItemPicker(Vector2 position);
	/*
	 * R, T, L, B => 0, 1, 2, 3
	 */
	public int GetSelectedIndex();
	public bool IsShowingItemPicker();
}

public interface IUIStatusIndicator
{
	public void ShowIndicator(bool show);
	public void UpdateIndicator(int health, int shield, string buff = "");
	public void UpdateIndicatorPosition(Vector2 position);
}

public interface IUICurtain
{
	public void Open();
	public void Close();
}

// deprecated
public interface IUIBuffSlotPanel
{
	public void AddBuff(string buff);
	public void RemoveBuff();
}