using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
public class UIBuffPanel : UIBehaviour, IUIBuffSlotPanel
{
	[SerializeField]
	GameObject _parent;
	[SerializeField]
	GameObject _buffPrefab;
	[SerializeField]
	float _queueTime;

	RectTransform _panelRT;
	Coroutine _shiftCo;
	Queue<string> _buffQueue = new();

	protected override void Awake()
	{
		_panelRT = GetComponent<RectTransform>();
	}

	public void Log()
	{
		GLogger.Log($"size delta {_panelRT.sizeDelta}");
		GLogger.Log($"mask child count {_parent.transform.childCount}");
	}

	public void AddBuff(string buff)
	{
		if (_shiftCo != null)
		{
			StopCoroutine(_shiftCo);
		}
		_shiftCo = StartCoroutine(EnqueueBuff(buff));
	}

	public void RemoveBuff()
	{
		throw new System.NotImplementedException();
	}

	IEnumerator EnqueueBuff(string buff)
	{
		_buffQueue.Enqueue(buff);
		var obj = Instantiate(_buffPrefab, new Vector2(280f, 0f),Quaternion.identity);
		obj.transform.SetParent(_parent.transform, false);
	
		obj.transform.localScale = new Vector2(1f, 1f);
		//obj.transform.position = new Vector2(280f, 0f);
		yield return null;

		float t = 0f;

		while (t >= _queueTime)
		{
			var x = Mathf.Lerp(280f, 0f, t /  _queueTime);
			obj.transform.position = new Vector2(x, 0f);

			t += Time.deltaTime;
			yield return null;
		}
	}

}
