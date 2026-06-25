using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Components;
using System.Collections;
using UnityEngine.Localization.Settings;



#if UNITY_EDITOR
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class UILoginPanel : UIBehaviour, IUILoginPannel
{
	[SerializeField]
	TMP_InputField _id;
	[SerializeField]
	TMP_InputField _password;
	[SerializeField]
	GameObject _tooltipPrefab;
	[SerializeField]
	Button _loginButton;
	[SerializeField]
	Button _audioDownloadButton;
	[SerializeField]
	Button _registerButton;
	[SerializeField]
	LocalizeStringEvent _locStringEvent;
	[SerializeField]
	TMP_Text _audioDownloadProgress;
	[SerializeField]
	Button _koreanButton;
	[SerializeField]
	Button _englishButton;
	[SerializeField]
	Button _clearCacheButton;
	[SerializeField]
	Button _guestLogin;

	// test ui
	[SerializeField]
	Button _testButton1;
	[SerializeField]
	Button _testButton2;
	[SerializeField]
	Button _testButton3;
	[SerializeField] 
	CanvasGroup _canvasGroup;

	UILoginSO _uiso;
	UITooltip _tooltip;
	AudioContainer _audioContainer;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILoginSOHolder>().Data;
		_uiso.LoginUI = this;
		_tooltip = Instantiate(_tooltipPrefab, transform).GetComponent<UITooltip>();
		_audioContainer = AudioContainer.Instance;
		_id.onValueChanged.AddListener(OnIdValueChanged);
		_password.onValueChanged.AddListener(OnPasswordValueChanged);
		_locStringEvent.StringReference.SetReference(
			"DefaultStringTable",
			"audio-exist");
		
		_loginButton.onClick.AddListener(OnClickLogin);
		_registerButton.onClick.AddListener(OnClickRegister);

		_audioDownloadButton.onClick.AddListener(() => {
			OnDownloadAudio();
			_audioDownloadButton.gameObject.SetActive(false);
		});

		_koreanButton.onClick.AddListener(() =>
		{
			_uiso.RaiseChangeLocalization(0);
		});

		_englishButton.onClick.AddListener(() =>
		{
			_uiso.RaiseChangeLocalization(1);
		});

		_clearCacheButton.onClick.AddListener(() =>
		{
			_uiso.RaiseClearCache();
		});

		_guestLogin.onClick.AddListener(() =>
		{
			_audioContainer.PlayAudio("click-mouse");
			_uiso.RaiseGuestLogin();
		});

		//test
		_testButton1.onClick.AddListener(() => {
			_uiso.RaiseTestEvent_1();
		});

		_testButton2.onClick.AddListener(() => {
			_uiso.RaiseTestEvent_2();
		});

		_testButton3.onClick.AddListener(() => {
			_uiso.ShowNotification($"fkdjskfjkdf {Random.Range(0f, 130f)}");
		});
	}
	protected override void OnDestroy()
	{
		_uiso.LoginUI = null;
	}

	public void SetId(string id)
	{
		_id.text = id;
	}

	public void SetPassword(string password)
	{
		_password.text = password;
	}

	void OnClickLogin()
	{
		_audioContainer.PlayAudio("click-mouse");
		if (_id.text.Length < 2)
		{
			_tooltip.ShowTooltip(
				"DefaultStringTable",
				"warning-id-too-short",
				_id.transform.position,
				UITooltip.AnchorPreset.MiddleTop);
			return;
		}

		if (_password.text.Length < 2)
		{
			_tooltip.ShowTooltip(
				"DefaultStringTable",
				"warning-password-too-short",
				_password.transform.position,
				UITooltip.AnchorPreset.MiddleTop);
			return;
		}

		_uiso.RaiseOnLogin(_id.text, _password.text);
	}

	void OnDownloadAudio()
	{
		_uiso.RaiseOnDownloadAudio();
	}

	void OnClickRegister()
	{
		_audioContainer.PlayAudio("click-mouse");
		_uiso.RaiseOnRegister();
	}

	void OnIdValueChanged(string value)
	{
		_audioContainer.PlayAudio("beep");
		_id.text = FilterText(value);
	}

	void OnPasswordValueChanged(string value)
	{
		_audioContainer.PlayAudio("beep");
		_password.text = FilterText(value);
	}

	string FilterText(string value)
	{
		return Regex.Replace(value, @"[^0-9a-zA-Z가-힣ㄱ-ㅎㅏ-ㅣ]", "");
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}

	public void ShowAudioDownloadButton(long size)
	{
		_audioDownloadButton.gameObject.SetActive(true);
		_locStringEvent.StringReference.SetReference(
			"DefaultStringTable",
			"audio-update");
		_locStringEvent.StringReference.Arguments
			= new[] { $"{size / (1024f * 1024f):F2}" };
		_locStringEvent.RefreshString();
	}

	public void SetAudioDownloadProgress(string progress)
	{
		_audioDownloadProgress.text = progress;
	}
}
