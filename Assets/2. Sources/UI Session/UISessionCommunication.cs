using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UISessionCommunication : UIBehaviour, IUISessionCommunication
{
	[SerializeField]
	Button _readyButton;
	[SerializeField]
	Button _leaveButton;
	[SerializeField]
	TMP_Text _readyButtonText;
	[SerializeField]
	Image _readyBorderImage;
	[SerializeField]
	UIGradientUpMaterialController _readyButtonBackground;
	[SerializeField]
	ScrollRect _scrollRect;
	[SerializeField]
	GameObject _chatMessagePrefab;
	[SerializeField]
	TMP_InputField _inputField;
	[SerializeField]
	CanvasGroup _chatGroup;
	[Space]
	[Header("Test Input Message")]
	[SerializeField]
	string _message;

	UISessionSO _uiso;
	bool _isHost;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.SessionCommunication = this;

		_inputField.onSubmit.AddListener(OnMessageSubmitted);

		_readyButton.onClick.AddListener(() => _uiso.RaiseOnClickReady());
		_leaveButton.onClick.AddListener(() => _uiso.RaiseOnClickLeave());
	}

	void OnMessageSubmitted(string message)
	{
		if (message.Length == 0)
		{
			_inputField.Select();
			_inputField.ActivateInputField();

			return;
		}

		_inputField.text = "";
		_inputField.Select();
		_inputField.ActivateInputField();

		_uiso.RaiseOnSubmitMessage(message);
	}

	public void AddMessage(string speaker, string message, Color personalColor)
	{
		var go = Instantiate(_chatMessagePrefab, _scrollRect.transform);
		var cm = go.GetComponent<UIChatMessage>();
		cm.SpeakerColor = personalColor;
		cm.messageColor = Color.white;
		cm.SetText(speaker, message);
		
		go.transform.SetParent(_scrollRect.content);

		LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);
		_scrollRect.verticalNormalizedPosition = 0f;
	}

	public void SetInteractable(bool interactable)
	{
		_chatGroup.interactable = interactable;
	}

	public void SetReadyButtonText(string text)
	{
		_readyButtonText.text = text;
	}

	public void SetReadyButtonHighlight(bool bright)
	{
		GLogger.Log($"set highlist {bright}");
		if (bright)
		{
			var c = new Color(0f, 218f / 255f, 1f);
			_readyBorderImage.color = c;
			_readyButtonBackground.Color = c;
		}
		else
		{
			var c = new Color(1f, 127f / 255f, 0f);
			_readyBorderImage.color = c;
			_readyButtonBackground.Color = c;
		}
	}

	public void TestChatMessage()
	{
		if (_message != null)
		{
			AddMessage("Editor", _message, Color.white);
		}
	}
}
