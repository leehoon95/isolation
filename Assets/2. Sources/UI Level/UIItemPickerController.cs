using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class UIItemPickerController : UIBehaviour, IUIItemPickerPanel
{
	[SerializeField]
	Canvas _canvas;
	[SerializeField]
	UIItemPicker _itemPick;
	[SerializeField]
	UILineConnector _lineConnector;

	UILevelSO _uiso;
	Vector2 _mouseDirection;
	
	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_uiso.ItemPicker = this;
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

			if (degree <= 45f || degree > 315f) // right
			{
				_itemPick.SelectedTile(0);
			}
			else if (degree <= 135f && degree > 45f) // top
			{
				_itemPick.SelectedTile(1);
			}
			else if (degree <= 225f && degree > 135f) // left
			{
				_itemPick.SelectedTile(2);
			}
			else if (degree <= 315f && degree > 225f) // bottom
			{
				_itemPick.SelectedTile(3);
			}
		}
	}

	public void HideItemPicker()
	{
		_itemPick.gameObject.SetActive(false);
		_lineConnector.gameObject.SetActive(false);
	}

	public void ShowItemPicker(Vector2 position)
	{
		var onScreenPosition = Camera.main.WorldToScreenPoint(position);
		_itemPick.gameObject.SetActive(true);
		//ItemPick.transform.position = position;
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
		_lineConnector.gameObject.SetActive(true);
		ConnectPickerToMouse();
	}

	public void ConnectPickerToMouse()
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
}
