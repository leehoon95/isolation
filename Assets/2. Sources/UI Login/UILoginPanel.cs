using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
	Button _registerButton;

	// test ui
	[SerializeField]
	Button _disconnectButton;
	[SerializeField]
	Button _sendAccountCreationMessage;
	[SerializeField]
	Button _notifyButton;
	[SerializeField] 
	CanvasGroup _canvasGroup;

	UILoginSO _uiso;
	UITooltip _tooltip;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILoginSOHolder>().Data;
		_uiso.LoginUI = this;
		_tooltip = Instantiate(_tooltipPrefab, transform).GetComponent<UITooltip>();

		_id.onValueChanged.AddListener(OnIdValueChanged);
		_password.onValueChanged.AddListener(OnPasswordValueChanged);
		
		_loginButton.onClick.AddListener(OnClickLogin);
		_registerButton.onClick.AddListener(OnClickRegister);

		//test
		_disconnectButton.onClick.AddListener(() => {
			_uiso.RaiseTestEvent_1();
		});

		_sendAccountCreationMessage.onClick.AddListener(() => {
			_uiso.RaiseTestEvent_2();
		});

		_notifyButton.onClick.AddListener(() => {
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
		GLogger.Log("onclicklogin");
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

	void OnClickRegister()
	{
		_uiso.RaiseOnRegister();
	}

	void OnIdValueChanged(string value)
	{
		_id.text = FilterText(value);
	}

	void OnPasswordValueChanged(string value)
	{
		_password.text = FilterText(value);
	}

	string FilterText(string value)
	{
		return Regex.Replace(value, @"[^0-9a-zA-Z°¡-ÆR¤¡-¤¾¤¿-¤Ó]", "");
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
		//_id.interactable = interactable;
		//_password.interactable = interactable;
		//_loginButton.interactable = interactable;
		//_registerButton.interactable = interactable;
	}
}
