using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;

public class UIItemPickerController : UIBehaviour, IUIItemPickerPanel
{
	[SerializeField]
	Canvas _canvas;
	[SerializeField]
	UIItemPicker _itemPick;
	[SerializeField]
	UILineConnector _lineConnector;

	UILevelSO _uiso;
	LocalizedString _localizedString;
	Vector2 _mouseDirection;
	bool _fronMode;
	int _selectedIndex;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_uiso.ItemPicker = this;
		_localizedString = new() { 
			TableReference = "DefaultStringTable",
		};

	}

	void Update()
	{
		if (_lineConnector.IsActive())
		{
			ConnectPickerToMouse();
			var degree = Mathf.Atan2(_mouseDirection.y, _mouseDirection.x) * Mathf.Rad2Deg;
			if (degree < 0f)
			{
				degree += 360f;
			}

			if (_fronMode)
			{
				if (degree <= 180f && degree > 0f)
				{
					_itemPick.SelectedTile(1);
					_selectedIndex = 1;
				}
				else
				{
					_itemPick.SelectedTile(3);
					_selectedIndex = 3;
				}
			}
			else
			{
				if (degree <= 90f || degree > 315f) // right
				{
					_itemPick.SelectedTile(0);
					_selectedIndex = 0;
				}
				//else if (degree <= 135f && degree > 45f) // top
				//{
				//	_itemPick.SelectedTile(1);
				//	_selectedIndex = 1;
				//}
				else if (degree <= 225f && degree > 90f) // left
				{
					_itemPick.SelectedTile(2);
					_selectedIndex = 2;
				}
				else if (degree <= 315f && degree > 225f) // bottom
				{
					_itemPick.SelectedTile(3);
					_selectedIndex = 3;
				}
			}
		}
	}

	public void HideItemPicker()
	{
		_itemPick.gameObject.SetActive(false);
		_lineConnector.gameObject.SetActive(false);
	}

	public void ShowItemPicker(Vector2 position, string itemEffect, bool frontItem)
	{
		
		_itemPick.gameObject.SetActive(true);
		_itemPick.FronMode = frontItem;
		_localizedString.TableEntryReference = $"weapon-{itemEffect}";
		_itemPick.ItemName = _localizedString.GetLocalizedString();
		_fronMode = frontItem;
		_lineConnector.gameObject.SetActive(true);

		MovePickerToPosition(position);
		//ConnectPickerToMouse();
	}

	void MovePickerToPosition(Vector2 position)
	{
		var onScreenPosition = Camera.main.WorldToScreenPoint(position);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_canvas.transform as RectTransform,
			onScreenPosition,
			_canvas.worldCamera,
			out Vector2 localPoint
			))
		{
			var rt = _itemPick.gameObject.transform as RectTransform;
			rt.anchoredPosition = localPoint;
			//GLogger.Log($"{position} {localPoint}");
		}
	}

	void ConnectPickerToMouse()
	{
		var mousePosition = Mouse.current.position.value;
		var uiCamera = _canvas.worldCamera;

		_lineConnector.startPoint = (_itemPick.transform as RectTransform).anchoredPosition;

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_canvas.transform as RectTransform,
			mousePosition,
			uiCamera,
			out Vector2 localPoint
			))
		{
			_lineConnector.endPoint = localPoint;
			//GLogger.Log($"{_lineConnector.startPoint} {localPoint}");
		}
		_lineConnector.SetAllDirty();
		_mouseDirection = (_lineConnector.endPoint - _lineConnector.startPoint).normalized;
	}

	public void MoveItemPicker(Vector2 position)
	{
		MovePickerToPosition(position);
	}

	public int GetSelectedIndex()
	{
		return _selectedIndex;
	}

	public bool IsShowingItemPicker()
	{
		return _itemPick.gameObject.activeInHierarchy;
	}
}
