using System.Text.RegularExpressions;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class LoginPanel : MonoBehaviour
{
	[SerializeField]
	TMP_InputField _nickNameInputField;
	[SerializeField]
	UILoginSO _uil;
	[SerializeField]
	Button _enterButton;
	[SerializeField]
	Button _disconnectButton;
	[SerializeField]
	Button _sendUDPDataButton;

	private void Start()
	{
		_nickNameInputField.onValueChanged.AddListener(OnNickNameValueChanged);
		//_nickNameInputField.onSubmit.AddListener(OnSubmit);

		//_sdl = FindAnyObjectByType<SaveDataLoader>();
		//if (_sdl != null)
		//{
		//	_nickNameInputField.text = _sdl.SaveData.nickName;
		//}

		_enterButton.onClick.AddListener(OnClickEnter);
		_disconnectButton.onClick.AddListener(OnClickDisconnect);
		_sendUDPDataButton.onClick.AddListener(() => {
			_uil.RaiseSendUDPData();
		});

		_uil.SetLoginPanelObject(this);
	}

	public void SetNickname(string nickname)
	{
		_nickNameInputField.text = nickname;
	}

	public void OnClickEnter()
	{
		if (_nickNameInputField.text.Length < 2)
		{
			_uil.ShowNoticeOnTop("´Ð³×ÀÓÀº 2±ÛÀÚ ÀÌ»ó ÀÔ·ÂÇØÁÖ¼¼¿ä.");
			return;
		}

		//print("OnEnter() called input text: " + _nickNameInputField.text);
		//_sdl.SaveData.nickName = _nickNameInputField.text;
		//_sdl.WriteSaveDataAsync().ContinueWith(task =>
		//{
		//	if (task.IsFaulted)
		//	{
		//		Debug.LogError("Failed to write save data: " + task.Exception);
		//	}
		//	else
		//	{
		//		Debug.Log("Save data written successfully.");
		//	}
		//});

		_uil.RaiseOnLoginEnter(_nickNameInputField.text);
	}

	public void OnClickDisconnect()
	{
		print("Disconnect From the server.");
		_uil.RaiseOnDisconnect();
	}

	void OnNickNameValueChanged(string value)
	{
		//print("OnNickNameValueChanged() called with value: " + value);
		FilteringNickName(value);
	}

	//void OnSubmit(string value)
	//{
	//	print("OnSubmit() called with value: " + value);
	//	FilteringNickName(value);
	//}

	void FilteringNickName(string value)
	{
		string filtered = Regex.Replace(value, @"[^0-9a-zA-Z°¡-ÆR¤¡-¤¾¤¿-¤Ó]", "");
		if (_nickNameInputField.text != filtered)
		{
			_nickNameInputField.text = filtered;
			//_nickNameInputField.
			//_nickNameInputField.caretPosition = filtered.Length;
		}
	}
}
