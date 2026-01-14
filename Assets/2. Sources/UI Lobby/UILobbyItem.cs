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

	string _lobbyId;
	bool _isPlaying;

	public event Action<string> OnClick;

	protected override void Start()
	{
		_button.onClick.AddListener(() => OnClick?.Invoke(_lobbyId));
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

	public void SetLobbyInfo(LobbySettings settings)
	{
		_buttonText.text = settings.Name;
		_slotText.text = $"{settings.MaxPlayers - settings.AvailableSlots} / {settings.MaxPlayers}";
		_lobbyId = settings.Id;
		_isPlaying = settings.IsPlaying;
	}

	public void ClearButtonEvent()
	{
		OnClick = null;
	}

	protected override void OnDestroy()
	{
		ClearButtonEvent();
	}
}
