using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UISessionItem : UIBehaviour
{
	[SerializeField]
	uint _padding;
	[SerializeField]
	Button _button;
	[SerializeField]
	TMP_Text _buttonText;
	[SerializeField]
	TMP_Text _slotText;

	int _maxPlayer;
	int _playerCount;
	string _lobbyCode;
	bool _interactable = false;

	Action<string> _onClick;
	public event Action<string> OnClick
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

	public bool State
	{
		get
		{
			return _interactable;
		}
		set
		{
			_interactable = value;
		}
	}

	protected override void Awake()
	{
		_button.onClick.AddListener(() => _onClick?.Invoke(_lobbyCode));
		_slotText.text = "0 / 4";
	}

	//public void FitSize(RectTransform parentRectTransform)
	//{
	//	RectTransform rectTransform = GetComponent<RectTransform>();

	//	rectTransform.SetSizeWithCurrentAnchors(
	//		RectTransform.Axis.Horizontal,
	//		parentRectTransform.rect.width - _padding * 2);
	//	rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
	//		40f);
	//}

	public void SetLobbyInfo(
		string name,
		int maxPlayerCount,
		int playerCount,
		string lobbyCode)
	{
		_buttonText.text = name;
		_slotText.text = $"{playerCount} / {maxPlayerCount}";
		_lobbyCode = lobbyCode;
	}

	public void ClearButtonEvent()
	{
		_onClick = null;
		_button.onClick.RemoveAllListeners();

	}

	protected override void OnDestroy()
	{
		ClearButtonEvent();
	}
}
