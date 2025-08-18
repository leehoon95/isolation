using System.Collections;
using System.Collections.Concurrent;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UINotificationPanel : UIBehaviour
{
	[SerializeField]
	GameObject _obj;
	[SerializeField]
	Animator _animator;
	[SerializeField]
	TMP_Text _noticeContent;
	[SerializeField]
	UINotificationBackground _notificationBackground;
	[SerializeField]
	UILoginSO _uil;

	Coroutine _cachedCoroutin;
	bool _processing;

	protected override void Awake()
	{
		base.Awake();
		_uil.SetNoticePanelObejct(this);
	}

	public void ShowNotice(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			return;
		}


		if (_processing && !_cachedCoroutin.IsUnityNull())
		{
			print("stop coroutin!!");
			StopCoroutine(_cachedCoroutin);
			
			_obj.SetActive(false);
			_animator.SetTrigger("Hide");
			//_animator.GetCurrentAnimatorClipInfo(0)[0].clip.
			
			//infos[0].clip.name
		}

		_cachedCoroutin = StartCoroutine(ProcessNotify(content));
	}

	IEnumerator ProcessNotify(string content)
	{
		_processing = true;
		_noticeContent.text = content;
		_obj.SetActive(true);
		_animator.SetTrigger("On");

		yield return new WaitForSeconds(3f);

		_animator.SetTrigger("Off");

		yield return new WaitForSeconds(.5f);

		_obj.SetActive(false);
		_processing = false;
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();

		if (_processing && !_cachedCoroutin.IsUnityNull())
		{
			StopCoroutine(_cachedCoroutin);

			_obj.SetActive(false);
		}
	}
}
