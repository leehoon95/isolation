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
	Button _yes;
	[SerializeField]
	Button _no;

	public event UnityAction OnYes;
	public event UnityAction OnNo;

	protected override void Start()
	{
		_yes.onClick.AddListener(() => OnYes?.Invoke());
		_no.onClick.AddListener(() => OnNo?.Invoke());

	}
	public void SetTitle(string title) => _title.text = title;
    public void SetContent(string content) => _content.text = content;
}
