using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class UITooltip : UIBehaviour
{
	[SerializeField]
	Image _tooltip;
	[SerializeField]
	TMP_Text _text;
	[SerializeField]
	[Range(0f, 5f)]
	float _fadeDuration = 1f;
	[SerializeField]
	[Range(1f, 5f)]
	float _duration = 3f;

	Coroutine _coroutine;

	public enum AnchorPreset
	{
		LeftTop, RightTop, LeftBottom, RightBottom, MiddleTop, MiddleBottom
	}

	protected override void OnEnable()
	{
		SetAlpha(0f);
	}

	public void ShowTooltip(
		string tableName,
		string key,
		Vector3 pos,
		AnchorPreset ap)
	{
		if (_coroutine != null)
		{
			SetAlpha(0f);
			StopCoroutine(_coroutine);
		}

		_coroutine = StartCoroutine(ShowingCoroutine(tableName, key, pos, ap));
	}

	IEnumerator ShowingCoroutine(
		string tableName,
		string key,
		Vector3 pos,
		AnchorPreset ap)
	{
		var op = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			tableName,
			key,
			LocalizationSettings.SelectedLocale);

		while (!op.IsDone)
		{
			yield return null;
		}

		_text.text = op.Result;
		var rt = (RectTransform)_tooltip.transform;
		switch (ap)
		{
			case AnchorPreset.LeftTop:
				rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.up;
				break;
			case AnchorPreset.RightTop:
				rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.one;
				break;
			case AnchorPreset.LeftBottom:
				rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
				break;
			case AnchorPreset.RightBottom:
				rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.right;
				break;
			case AnchorPreset.MiddleTop:
				rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 1f);
				break;
			case AnchorPreset.MiddleBottom:
				rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0f);
				break;
		}
		rt.anchoredPosition = Vector2.zero;
		transform.position = pos;

		float timer = 0f;

		while (timer < _fadeDuration)
		{
			timer += Time.deltaTime;
			SetAlpha(timer);

			yield return null;
		}

		yield return new WaitForSeconds(_duration);

		timer = _fadeDuration;
		while (timer > 0f)
		{
			timer -= Time.deltaTime;
			SetAlpha(timer);

			yield return null;
		}

		_coroutine = null;
	}

	void SetAlpha(float time)
	{
		if (time < 0f)
		{
			time = 0f;
		}

		float a = Mathf.Lerp(0f, 1f, time / _fadeDuration);
		_tooltip.color = new Color(1f, 1f, 1f, a);
		_text.color = new Color(0f, 0f, 0f, a);
	}
}
