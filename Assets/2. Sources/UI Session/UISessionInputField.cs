using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(TMP_InputField))]
public class UISessionInputField : UIBehaviour
{
    TMP_InputField _inputField;
	UISessionSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UISessionSOHolder>().Data;
		_inputField = GetComponent<TMP_InputField>();
		_inputField.onSubmit.AddListener(OnSubmit);
		_inputField.characterLimit = 200;
	}

	void OnSubmit(string message)
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
}
