using System;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogOk : UIBehaviour
{
	[SerializeField]
    TMP_Text _title;
    [SerializeField]
    TMP_Text _content;
	[SerializeField]
	Button _ok;
	[SerializeField]
	TMP_Text _okButtonText;

	// 가능하면 직접 구독 해제할 것
	public event UnityAction OnOk;
	AudioContainer _audioContainer;

	protected override void Start()
	{
		_audioContainer = AudioContainer.Instance;
		_ok.onClick.AddListener(OnClickButton);
	}

	protected override void OnEnable()
	{
		_ok.animator.Play("Normal", -1, 0f);
	}

	protected override void OnDisable()
	{
		OnOk = null;
	}

	void OnClickButton()
	{
		_audioContainer.PlayAudio("click-mouse");
		OnOk?.Invoke();
	}

	public void SetTitle(string title) => _title.text = title;

    public void SetContent(string content) => _content.text = content;
	
	public void SetOkButtonText(string text) => _okButtonText.text = text;
	
}
