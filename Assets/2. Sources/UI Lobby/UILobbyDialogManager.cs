using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobbyDialogManager : UIBehaviour, IUILobbyDialogManager
{
	[SerializeField] Button _cancelButton;
	[SerializeField] UIDialogCreateLobby _dialogCreateLobby;
	[SerializeField] CanvasGroup _canvasGroup;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.DialogManager = this;
	}

	public void SetOnCancelDialog(UnityAction onCancel)
	{
		_cancelButton.onClick.RemoveAllListeners();
		if (onCancel != null)
		{
			_cancelButton.onClick.AddListener(onCancel);
		}
	}

	public void ShowLobbyCreationDialog(UnityAction<string, string> onSubmit)
	{
		_cancelButton.gameObject.SetActive(true);
		_dialogCreateLobby.gameObject.SetActive(true);
		_dialogCreateLobby.OnSubmit += onSubmit;
	}

	public void HideLobbyCreationDialog()
	{
		_cancelButton.gameObject.SetActive(false);
		_dialogCreateLobby.gameObject.SetActive(false);
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}
}
