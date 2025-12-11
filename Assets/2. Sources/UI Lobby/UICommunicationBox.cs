using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// deprecated
public class UICommunicationBox : UIBehaviour//, IUICommunicationBox
{
    [SerializeField]
    ScrollRect _userList;
    [SerializeField]
    ScrollRect _messageList;
	[SerializeField]
    TMP_InputField _inputMessageField;

	UILobbySO _uiso;

	protected override void Start()
	{
        base.Start();

		//_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		//_uiso.CommunicationBox = this;

		_inputMessageField.onEndEdit.AddListener(OnEndEdit);
	}

    void OnEndEdit(string message)
    {
        print($"message: {message}");

        _inputMessageField.text = "";

        _uiso.RaiseOnEndEditMessage(message);
    }
}
