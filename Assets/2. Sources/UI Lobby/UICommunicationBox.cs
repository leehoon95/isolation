using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UICommunicationBox : UIBehaviour, ICommunicationBoxUI
{
    [SerializeField]
    UILobbySO _uiso;
    [SerializeField]
    ScrollRect _userList;
    [SerializeField]
    ScrollRect _messageList;
	[SerializeField]
    TMP_InputField _inputMessageField;

	protected override void Start()
	{
		_uiso.CommunicationBox = this;

		_inputMessageField.onEndEdit.AddListener(OnEndEdit);
	}

    void OnEndEdit(string message)
    {
        print($"message: {message}");

        _inputMessageField.text = "";

        _uiso.RaiseOnEndEditMessage(message);
    }
}
