using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobbyDialogManager : UIBehaviour, ILobbyDialogManager
{
	
	[SerializeField] Button _cancelButton;
	[SerializeField] UIDialogCreateRoom _dialogCreateRoom;

	UILobbySO _uiso;
	GameObject _openedDialog;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.DialogManager = this;
		_cancelButton.onClick.AddListener(() => _uiso.RaiseOnCancelDialog());
	}

	public void CloseDialog()
	{
		if (_openedDialog != null)
		{
			_openedDialog.SetActive(false);
			_cancelButton.gameObject.SetActive(false);

			_openedDialog = null;
		}
	}

	public void OpenDialog_CR()
	{
		if (_openedDialog != null)
		{
			return;
		}
		else
		{
			_openedDialog = _dialogCreateRoom.gameObject;
		}

		_dialogCreateRoom.gameObject.SetActive(true);
		_cancelButton.gameObject.SetActive(true);
	}

	public void SetTitle_CR(string title) => _dialogCreateRoom.SetTitle(title);

	public void SetContent_CR(string content) => _dialogCreateRoom.SetContent(content);

	public void AddOnOk_CR(UnityAction<string, string> ua) => _dialogCreateRoom.OnOk += ua;

	public void RemoveOnOk_CR(UnityAction<string, string> ua) => _dialogCreateRoom.OnOk -= ua;


}
