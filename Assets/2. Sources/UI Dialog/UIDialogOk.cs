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

	Animator _buttonAnimator;

	//public event UnityAction OnOk
	//{
	//	add
	//	{
	//		_onOk += value;
	//	}

	//	remove
	//	{
	//		_onOk -= value;
	//	}
	//}

	protected override void Start()
	{
		_ok.onClick.AddListener(() => OnOk?.Invoke());
	}

	protected override void OnEnable()
	{
		var animator = _ok.gameObject.GetComponent<Animator>();
		if (animator != null)
		{
			Debug.Log("rebind");
			animator.Play("Normal", -1, 0f);
		}
	}

	public void SetTitle(string title) => _title.text = title;

    public void SetContent(string content) => _content.text = content;
	
	public void SetOkButtonText(string text) => _okButtonText.text = text;
	
}
