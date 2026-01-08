using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UILobbyPlayerLabel : UIBehaviour, IUILobbyPlayerLabel
{
    [SerializeField]
    TMP_Text _label;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.PlayerLabel = this;
	}

	public void SetPlayerLabel(string nickname, Color personalColor)
	{
		_label.text = nickname;
		_label.color = personalColor;
	}
}
