using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum RoomState
{
	Open,
	Disabled
}

//[ExecuteAlways]

public class SessionItem : UIBehaviour
{
	[SerializeField]
	uint _padding;
	[SerializeField]
	Button _button;
	[SerializeField]
	TMP_Text _buttonText;
	[SerializeField]
	TMP_Text _slotText;

	int _sessionIndex;
	int _maxClientCount;
	int _clientCount;
	string _password;
	string _joinCode;

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

	protected override void Awake()
	{
		Debug.Log("SessionItem.Start()");
		_button.onClick.AddListener(() => _onClick?.Invoke(_sessionIndex));
		_slotText.text = "0 / 4";
	}

	public void FitSize(RectTransform parentRectTransform)
	{
		RectTransform rectTransform = GetComponent<RectTransform>();

		rectTransform.SetSizeWithCurrentAnchors(
			RectTransform.Axis.Horizontal,
			parentRectTransform.rect.width - _padding * 2);
		rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
			40f);
	}

	public void SetSessionInfo(
		int sessionIndex,
		string name,
		int maxClientCount,
		int clientCount,
		string password,
		string joinCode)
	{
		_sessionIndex = sessionIndex;
		_buttonText.text = name;
		_slotText.text = $"{clientCount} / {maxClientCount}";
		_password = password;
		_joinCode = joinCode;

		Debug.LogWarning($"slottext {_slotText.text}");
	}

	public void ClearButtonEvent()
	{
		_onClick = null;
		_button.onClick.RemoveAllListeners();

	}

	protected override void OnDestroy()
	{
		_button.onClick.RemoveAllListeners();
		_onClick = null;
	}
}
