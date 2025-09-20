using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class UINotification : UIBehaviour, INotificationUI
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
	[InterfaceType(typeof(ISupportNotificationUI))]
	ScriptableObject _SOSupportNotification;

	ISupportNotificationUI _notification;

	Coroutine _cachedCoroutin;
	bool _processing;

	protected override void Awake()
	{
		_notification = _SOSupportNotification as ISupportNotificationUI;
		if (_notification == null && _SOSupportNotification != null)
		{
			Debug.LogError("할당된 오브젝트가 ISupportNotificationUI를 구현하지 않음", this);
		}
		else
		{
			_notification.Notification = this;
		}
	}
	protected override void OnDestroy()
	{
		if (_processing && _cachedCoroutin != null)
		{
			StopCoroutine(_cachedCoroutin);

			_obj.SetActive(false);
		}

		_notification.Notification = null;
	}

	public void ShowNotification(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			return;
		}


		if (_processing && _cachedCoroutin != null)
		{
			//print("stop coroutin!!");
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
}
