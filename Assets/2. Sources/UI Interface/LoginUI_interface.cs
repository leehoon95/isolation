using UnityEngine;
using UnityEngine.Events;

public class AccountCreationApplication
{
	public string id;
	public string password;
	public string nickname;
	public uint h; // hue
	public uint s; // saturation
	public uint v; // value
}

public interface IUILoginPannel
{
	public void SetId(string Id);
	public void SetPassword(string password);
	public void SetInteractable(bool interactable);
	public void ShowAudioDownloadButton(long size);
	public void SetAudioDownloadProgress(string progress);
}

public interface IUILoginDialogManager
{
	public void SetOnCancelDialog(UnityAction onCancel);

	public void ShowOkDialog(
	string title,
	string content,
	string okButton,
	UnityAction onOk);
	public void HideOkDialog();

	public void ShowYesNoDialog(
	string title,
	string content,
	string yesButton,
	string noButton,
	UnityAction onYes,
	UnityAction onNo);
	public void HideYesNoDialog();

	public void ShowAccountCreationDialog(UnityAction<AccountCreationApplication> onSubmit);
	public void HideAccountCreationDialog();
	public void SetAccountCreationDialogOkButtonWaiting(bool waiting);
	public void SetInteractable(bool interactable);
}

