using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum RoomState
{
	Open,
	Disabled
}

[ExecuteAlways]
public class RoomItem : MonoBehaviour
{
	[SerializeField]
	uint _padding;
	[SerializeField]
	Button _button;
	[SerializeField]
	TMP_Text _buttonText;
	[SerializeField]
	TMP_Text _slotText;

	Action<int> _onClick;
	public event Action<int> OnClick
	{
		add
		{
			_onClick -= value;
			_onClick += value;
		}
		remove
		{
			_onClick -= value;
		}
	}

	public int RoomIndex { get; set; }
	public string RoomName
	{
		get
		{
			return _buttonText.text;
		}
		set
		{
			_buttonText.text = value;
		}

	}

	RoomState _roomState;
	public RoomState State
	{
		get
		{
			return _roomState;
		}
		set
		{
			_roomState = value;
			switch (value)
			{
				case RoomState.Open:
					_button.interactable = true;
					break;
				case RoomState.Disabled:
					_button.interactable = false;
					break;
			}
		}
	}

	int _clientCount;
	public int ClientCount
	{
		get
		{
			return _clientCount;
		}
		set
		{
			_clientCount = value;
			_slotText.text = $"{value} / 4";
		}
	}

	void Start()
	{
		_button.onClick.AddListener(() => _onClick?.Invoke(RoomIndex));
		//State = RoomState.Open;
		_slotText.text = "0 / 4";
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
		_onClick = null;
		_button.onClick.RemoveAllListeners();

	}

	void OnDestroy()
	{
		_button.onClick.RemoveAllListeners();
		_onClick = null;
	}
}
