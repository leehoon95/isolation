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
	ScrollRect _scrollRect;
	[SerializeField]
	GameObject _chatMessagePrefab;
	[SerializeField]
	TMP_InputField _inputField;
	[SerializeField]
	CanvasGroup _chatGroup;

	UISessionSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_uiso.SessionCommunication = this;

		_inputField.onSubmit.AddListener(OnMessageSubmitted);
		_inputField.characterLimit = 200;

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
}
