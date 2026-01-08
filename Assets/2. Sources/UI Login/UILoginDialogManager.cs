using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILoginDialogManager : UIBehaviour, IUILoginDialogManager
{
	[SerializeField] Button _cancelButton;
	[SerializeField] UIDialogOk _dialogOk;
	[SerializeField] UIDialogYesNo _dialogYesNo;
	[SerializeField] UIDialogCreateAccount _dialogCreateAccount;
	[SerializeField] CanvasGroup _canvasGroup;

	UILoginSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILoginSOHolder>().Data;
		_uiso.DialogManager = this;
	}

	// dialog cancel button action
	public void SetOnCancelDialog(UnityAction onCancel)
	{
		_cancelButton.onClick.RemoveAllListeners();
		if (onCancel != null)
		{
			_cancelButton.onClick.AddListener(onCancel);
		}
	}

	public void ShowOkDialog(
		string title, 
		string content, 
		string okButton, 
		UnityAction onOk)
	{
		_cancelButton.gameObject.SetActive(true);
		_dialogOk.gameObject.SetActive(true);
		_dialogOk.SetTitle(title);
		_dialogOk.SetContent(content);
		_dialogOk.SetOkButtonText(okButton);
		_dialogOk.OnOk += onOk;
	}

	public void HideOkDialog()
	{
		_cancelButton.gameObject.SetActive(false);
		_dialogOk.gameObject.SetActive(false);
	}

	public void ShowYesNoDialog(
		string title, 
		string content, 
		string yesButton, 
		string noButton, 
		UnityAction onYes, 
		UnityAction onNo)
	{
		_dialogYesNo.gameObject.SetActive(true);
		_dialogYesNo.SetTitle(title);
		_dialogYesNo.SetContent(content);
		_dialogYesNo.SetYesButtonText(yesButton);
		_dialogYesNo.SetNoButtonText(noButton);
		_dialogYesNo.OnYes += onYes;
		_dialogYesNo.OnNo += onNo;
	}

	public void HideYesNoDialog()
	{
		_dialogYesNo.gameObject.SetActive(false);
	}

	public void ShowAccountCreationDialog(
		UnityAction<AccountCreationApplication> onSubmit)
	{
		_cancelButton.gameObject.SetActive(true);
		_dialogCreateAccount.gameObject.SetActive(true);
		_dialogCreateAccount.OnSubmit += onSubmit;
	}

	public void HideAccountCreationDialog()
	{
		_cancelButton.gameObject.SetActive(false);
		_dialogCreateAccount.gameObject.SetActive(false);
	}

	public void SetAccountCreationDialogOkButtonWaiting(bool waiting)
	{
		_dialogCreateAccount.SetOkButtonWaiting(waiting);
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}
}
