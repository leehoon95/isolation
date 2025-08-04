using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CommunicationBox : MonoBehaviour
{
    [SerializeField]
    UILobbySO _uiSO;
    [SerializeField]
    ScrollRect _userList;
    [SerializeField]
    ScrollRect _messageList;
	[SerializeField]
    TMP_InputField _inputMessageField;

	void Start()
	{
        _uiSO.SetCommunicationBox(this);

        _inputMessageField.onEndEdit.AddListener(OnEndEdit);
	}

    void OnEndEdit(string message)
    {
        print($"message: {message}");

        _inputMessageField.text = "";

        _uiSO.RaiseOnEndEditMessage(message);
    }
}
