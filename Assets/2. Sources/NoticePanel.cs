using System.Collections;
using System.Collections.Concurrent;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class NoticePanel : MonoBehaviour
{
	[SerializeField]
	GameObject _obj;
	[SerializeField]
	Animator _animator;
	[SerializeField]
	TMP_Text _noticeContent;
	[SerializeField]
	UILoginSO _uil;
	IEnumerator _cachedCoroutin;
	bool _processing;

	void Start()
	{
		_uil.SetNoticePanelObejct(this);
	}

	public void ShowNotice(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			return;
		}

		if (_processing && _cachedCoroutin != null)
		{
			StopCoroutine(_cachedCoroutin);
			
			_obj.SetActive(false);
			_animator.SetBool("IsOn", false);
		}

		_cachedCoroutin = ProcessNotify(content);

		StartCoroutine(_cachedCoroutin);
	}

	IEnumerator ProcessNotify(string content)
	{
		_processing = true;
		_noticeContent.text = content;
		_obj.SetActive(true);
		_animator.SetBool("IsOn", true);

		yield return new WaitForSeconds(2.5f);

		_animator.SetBool("IsOn", false);

		yield return new WaitForSeconds(.5f);

		_obj.SetActive(false);
		_processing = false;
	}
}
