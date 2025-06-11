using TMPro;
using UnityEngine;

public class MessageSender : MonoBehaviour
{
	[SerializeField]
	TMP_InputField inputField;
	[SerializeField]
	ChatPannel chat;
	[SerializeField]
	Server server;

	void Start()
	{
		inputField.onSubmit.AddListener(OnSubmit);
	}

	public void OnSubmit(string message)
	{
		if (message.Length == 0)
		{
			inputField.Select();
			inputField.ActivateInputField();

			return;
		}

		inputField.text = "";
		inputField.Select();
		inputField.ActivateInputField();
		chat.ShowMessage(message);

		server.SendMessageToServer(message);
		//print($"input: {message}");
	}

	void Update()
	{
		
	}
}
