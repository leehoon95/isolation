using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

enum RoomState
{

}

[ExecuteAlways]
public class RoomItem : MonoBehaviour
{
	[SerializeField]
	uint _padding;
	[SerializeField]
	Button _enterButton;

	Action<int> _onEntry;
	public event Action<int> OnEntry
	{
		add
		{
			_onEntry -= value;
			_onEntry += value;
		}
		remove
		{
			_onEntry -= value;
		}
	}
	public int RoomIndex { get; set; }

	void Start()
	{
		_enterButton.onClick.AddListener(() => _onEntry?.Invoke(RoomIndex));
	}

	public void FitSize(RectTransform parentRectTransform)
	{
		//RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();
		RectTransform rectTransform = GetComponent<RectTransform>();

		rectTransform.SetSizeWithCurrentAnchors(
			RectTransform.Axis.Horizontal,
			parentRectTransform.rect.width - _padding * 2);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
			40f);
	}

	public void ClearButtonEvent()
	{
		_onEntry = null;
		_enterButton.onClick.RemoveAllListeners();

	}

	void OnDestroy()
	{
		_enterButton.onClick.RemoveAllListeners();
		_onEntry = null;
	}
}
