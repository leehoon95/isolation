using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIStatusIndicator : UIBehaviour, IUIStatusIndicator
{
	[SerializeField]
	Canvas _canvas;
	[SerializeField]
	CanvasGroup _canvasGroup;
	[SerializeField]
	GameObject _indicator;
	[SerializeField]
	UIIndicatorBarContainer _healthIndicator;
	[SerializeField]
	UIIndicatorBarContainer _shieldIndicator;
	[SerializeField]
	Image _burstImage;

	UILevelSO _uiso;
	Coroutine _visibilityCo;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_uiso.StatusIndicator = this;
	}

	public void UpdateIndicator(int health, int shield, string buff = "")
	{
		//GLogger.Log($"UpdateIndicator {health} {shield}");
		_healthIndicator.Count = health;
		_shieldIndicator.Count = shield;

		if (buff != null)
		{
			if (buff == "burst")
			{
				_burstImage.gameObject.SetActive(true);
			}
			else
			{
				_burstImage.gameObject.SetActive(false);
			}
		}

		if (_visibilityCo != null)
		{
			StopCoroutine(_visibilityCo);
		}
		_visibilityCo = StartCoroutine(ClearlyVisibleStatus(1.5f));
	}

	public void ShowIndicator(bool show)
	{
		_indicator.SetActive(show);
	}

	IEnumerator ClearlyVisibleStatus(float time)
	{
		_canvasGroup.alpha = 1f;
		var duration = new WaitForSeconds(time);
		yield return duration;

		var t = 0f;
		while (t < 1f)
		{
			_canvasGroup.alpha = Mathf.Lerp(1f, 0.2f, t);
			t += Time.deltaTime;
			yield return null;
		}

		_visibilityCo = null;
	}

	public void UpdateIndicatorPosition(Vector2 position)
	{
		var onScreenPosition = Camera.main.WorldToScreenPoint(position);
		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
			_canvas.transform as RectTransform,
			onScreenPosition,
			_canvas.worldCamera,
			out Vector2 localPoint
			))
		{
			var rt = gameObject.transform as RectTransform;
			rt.anchoredPosition = localPoint;
		}
	}


}
