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
	}

	protected override void OnDisable()
	{
		OnClick = null;
	}

	public void SetLobbyInfo(LobbySettings settings)
	{
		_buttonText.text = settings.Name;
		_slotText.text = $"{settings.MaxPlayers - settings.AvailableSlots} / {settings.MaxPlayers}";
		_lobbyId = settings.Id;
		_isPlaying = settings.IsPlaying;
	}
}
