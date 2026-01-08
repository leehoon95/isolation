using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogYesNo : UIBehaviour
{
    [SerializeField]
    TMP_Text _title;
    [SerializeField]
    TMP_Text _content;
	[SerializeField]
	Button _yesButton;
	[SerializeField]
	Button _noBuutton;
	[SerializeField]
	TMP_Text _yesButtonText;
	[SerializeField]
	TMP_Text _noButtonText;

	public event UnityAction OnYes;
	public event UnityAction OnNo;

	protected override void Start()
	{
		_yesButton.onClick.AddListener(OnYes);
		_noBuutton.onClick.AddListener(OnNo);
	}

	protected override void OnDisable()
	{
		OnYes = null;
		OnNo = null;
	}

	public void SetTitle(string title) => _title.text = title;
    public void SetContent(string content) => _content.text = content;
	public void SetYesButtonText(string text) => _yesButtonText.text = text;
	public void SetNoButtonText(string text) => _noButtonText.text = text;
}
