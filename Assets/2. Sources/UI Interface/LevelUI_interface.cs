using UnityEngine;

public interface IUIItemPickerPanel
{
	public void ShowItemPicker(Vector2 position, string itemEffect, bool onlyFront = false);
	public void HideItemPicker();
	/*
	 * R, T, L, B => 0, 1, 2, 3
	 */
	public int GetSelectedIndex();
	public bool IsShowingItemPicker();
}
