using JetBrains.Annotations;
using System;
using System.Collections;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UIDialogCreateAccount : UIBehaviour
{
	[SerializeField] TMP_Text _title;
	[SerializeField] TMP_InputField _id;
	[SerializeField] TMP_InputField _password;
	[SerializeField] TMP_InputField _nickname;
	[SerializeField] Toggle _showPasswordToggle;
	[SerializeField] TMP_Text _content;
	[SerializeField] Button _ok;
	[SerializeField] TMP_Text _okButtonText;
	[SerializeField] Slider _colorSlider;
	[SerializeField] Slider _saturationSlider;
	[SerializeField] Image _personalColorIndicator;
	[SerializeField] GameObject _tooltipPrefab;

	string _realPassword;
	public event UnityAction<AccountCreationApplication> OnSubmit;
	UITooltip _tooltip;
	uint _h = 0;
	uint _s = 255;
	Coroutine _taskCo;
	AudioContainer _ac;
	long _tick;

	protected override void Start()
	{
		_ac = AudioContainer.Instance;
		_id.onValueChanged.AddListener(IDFiltering);
		_password.onValueChanged.AddListener(PasswordFiltering);
		_password.asteriskChar = '*';
		_nickname.onValueChanged.AddListener(NicknameFiltering);
		_showPasswordToggle.isOn = true;
		_showPasswordToggle.onValueChanged.AddListener(OnToggleChanged);
		_colorSlider.onValueChanged.AddListener((float value) =>
		{
			var now = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
			if (now - _tick > 50)
			{
				_ac.PlayAudio("gear");
				_tick = now;
			}
			_h = (uint)Math.Round(value);
			_personalColorIndicator.color 
				= Color.HSVToRGB(value / 255f, (_saturationSlider.value / 255f), 1f);
		});
		_saturationSlider.onValueChanged.AddListener((float value) =>
		{
			var now = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
			if (now - _tick > 50)
			{
				_ac.PlayAudio("gear");
				_tick = now;
			}
			_s = (uint)Math.Round(value);
			_personalColorIndicator.color 
				= Color.HSVToRGB(_colorSlider.value / 255f, (value / 255f), 1f);
		});

		_tooltip = Instantiate(_tooltipPrefab, transform).GetComponent<UITooltip>();

		_ok.onClick.AddListener(() => {
			//if (_id.text.Length < 2)
			//{
			//	_tooltip.ShowTooltip(
			//		"DefaultStringTable", 
			//		"warning-id-too-short", 
			//		_id.transform.position, 
			//		UITooltip.AnchorPreset.LeftTop);
			//	return;
			//}

			//if (_password.text.Length < 2)
			//{
			//	_tooltip.ShowTooltip(
			//		"DefaultStringTable",
			//		"warning-password-too-short",
			//		_password.transform.position,
			//		UITooltip.AnchorPreset.LeftTop);
			//	return;
			//}

			//if (_nickname.text.Length < 2)
			//{
			//	_tooltip.ShowTooltip(
			//		"DefaultStringTable",
			//		"warning-nickname-too-short",
			//		_password.transform.position,
			//		UITooltip.AnchorPreset.LeftTop);
			//	return;
			//}
			_ac.PlayAudio("click-mouse");
			OnSubmit?.Invoke(new AccountCreationApplication()
			{
				id = _id.text,
				password = _password.text,
				nickname = _nickname.text,
				h = _h,
				s = _s,
				v = 255
			});
		});
	}

	protected override void OnEnable()
	{
		_id.text = "";
		_password.text = "";
		_nickname.text = "";
		_personalColorIndicator.color = Color.HSVToRGB(0.5f, 1f, 1f);
		_colorSlider.value = 127f;
		_saturationSlider.value = 192f;
		SetOkButtonWaiting(false);
	}

	protected override void OnDisable()
	{
		OnSubmit = null;
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
		}
	}

	void ShowTooltipAsync(string key, Vector3 pos, UITooltip.AnchorPreset ap)
	{
		_tooltip.ShowTooltip("DefaultStringTable", key, pos, ap);
	}

	IEnumerator Localize(int index)
	{
		/*
		 * 언어 세팅 초기화 완료를 기다림
		 * wait for the localization system to initialize, loading locales, preloading etc.
		 */
		yield return LocalizationSettings.InitializationOperation;
		LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[index];

		GLogger.Log($"Set kor locale {LocalizationSettings.StringDatabase.GetLocalizedString("DefaultStringTable", "localization-korean", LocalizationSettings.SelectedLocale)}");
	}

	void IDFiltering(string value)
	{
		_ac.PlayAudio("beep");
		string filtered = Regex.Replace(value, @"[^0-9a-zA-Z]", "");
		if (_id.text != filtered)
		{
			_id.text = filtered;
		}
	}

	void PasswordFiltering(string value)
	{
		_ac.PlayAudio("beep");
		string filtered = Regex.Replace(value, @"[^0-9a-zA-Z*]", "");
		if (_realPassword != filtered)
		{
			_realPassword = filtered;
		}
	}

	void NicknameFiltering(string value) 
	{
		_ac.PlayAudio("beep");
		string filtered = Regex.Replace(value, @"[^0-9a-zA-Z가-힣ㄱ-ㅎㅏ-ㅣ]", "");
		if (_nickname.text != filtered)
		{
			_nickname.text = filtered;
		}
	}

	void OnToggleChanged(bool on)
	{
		_ac.PlayAudio("beep");
		if (on)
		{
			_password.contentType = TMP_InputField.ContentType.Standard;
		}
		else
		{
			_password.contentType = TMP_InputField.ContentType.Password;
			
		}
		_password.ForceLabelUpdate();
	}

	public void SetOkButtonWaiting(bool waitting)
	{
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
		}

		if (waitting)
		{
			_ok.interactable = false;
			_taskCo = StartCoroutine(SetOkButtonText("waiting-account-creation-response"));
		}
		else
		{
			_ok.interactable = true;
			_taskCo = StartCoroutine(SetOkButtonText("creation"));
		}
	}

	IEnumerator SetOkButtonText(string localizationKey)
	{
		var task = LocalizationSettings.StringDatabase.GetLocalizedStringAsync(
			"DefaultStringTable", localizationKey, LocalizationSettings.SelectedLocale);
		yield return task;
		_okButtonText.text = task.Result;
		_taskCo = null;
	}
}
