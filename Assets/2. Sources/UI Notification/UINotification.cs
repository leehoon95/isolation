using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UINotification : UIBehaviour, INotificationUI
{
	[SerializeField]
	GameObject _mask;
	[SerializeField]
	Animator _animator;
	[SerializeField]
	TMP_Text _noticeContent;

	Coroutine _cachedCoroutin;

	protected override void OnDisable()
	{
		if (_cachedCoroutin != null)
		{
			StopCoroutine(_cachedCoroutin);

			_mask.SetActive(false);
		}
	}

	public void ShowNotification(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			return;
		}

		if (_cachedCoroutin != null)
		{
			StopCoroutine(_cachedCoroutin);

			_animator.SetTrigger("Off");
			_mask.SetActive(false);
		}

		_cachedCoroutin = StartCoroutine(ProcessNotifyText(content));
	}

	IEnumerator ProcessNotifyText(string content)
	{
		yield return null;

		_noticeContent.text = content;
		_mask.SetActive(true);
		//_animator.SetTrigger("On");
		_animator.Play("NoticePanelOn", -1, 0f);

		yield return new WaitForSeconds(1f);

		//_animator.SetTrigger("Off");
		_animator.Play("NoticePanelOff", -1, 0f);
		yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);

		_mask.SetActive(false);
	}
}
