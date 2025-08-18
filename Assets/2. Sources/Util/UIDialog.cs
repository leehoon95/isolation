using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum UIDialogType
{
    Ok,
    YesNo
}

public class UIDialog : UIBehaviour
{
	[SerializeField]
	GameObject _ui;
	[SerializeField]
	UILoginSO _uil;
    [SerializeField]
    Button _ok;
    [SerializeField]
    Button _yes;
    [SerializeField]
    Button _no;
    [SerializeField]
    TMP_Text _title;
    [SerializeField]
    TMP_Text _content;
	[SerializeField]
	TMP_Text _okButtonText;
    [SerializeField]
    UIDialogType _type;

	protected override void OnEnable()
	{
		base.OnEnable();

		_uil.SetDialogObejct(this);
		SetDialogButtonType();
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();

		SetDialogButtonType();
	}
#endif

    public void SetDialogButtonType()
    {
		if (_type == UIDialogType.Ok)
		{
			_ok.interactable = true;
			_ok.gameObject.SetActive(true);
			_yes.interactable = false;
			_yes.gameObject.SetActive(false);
			_no.interactable = false;
			_no.gameObject.SetActive(false);
		}
		else
		{
			_ok.interactable = false;
			_ok.gameObject.SetActive(false);
			_yes.interactable = true;
			_yes.gameObject.SetActive(true);
			_no.interactable = true;
			_no.gameObject.SetActive(true);
		}
	}

	public void SetTitle(string title) => _title.text = title;

    public void SetContent(string content) => _content.text = content;

	public void SetType(UIDialogType type)
	{
		_type = type;
		SetDialogButtonType();
	}
	
	public void SetOKButtonText(string text)
	{
		_okButtonText.text = text;
	}

	public void SetOKButtonOnClickListner(UnityAction listner) => _ok.onClick.AddListener(listner);
	public void RemoveOkButtonOnClickListner(UnityAction listner) => _ok.onClick.RemoveListener(listner);
	public void ShowDialog(bool show)
	{
		_ui.SetActive(show);
	}
}
