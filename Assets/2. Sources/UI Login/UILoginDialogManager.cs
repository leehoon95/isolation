using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UILoginDialogManager : UIBehaviour, ILoginDialogManager
{
	[SerializeField] UIDialogOk _dialogOk;
	[SerializeField] UIDialogYesNo _dialogYesNo;

	UILoginSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILoginSOHolder>().Data;
		_uiso.DialogManager = this;
	}

	public void SetActive_Ok(bool active) => _dialogOk.gameObject.SetActive(active);
	public void SetTitle_Ok(string title) => _dialogOk.SetTitle(title);
	public void SetContent_Ok(string content) => _dialogOk.SetContent(content);
	public void SetOkButtonText_Ok(string text) => _dialogOk.SetOkButtonText(text);
	public void AddOnOk_Ok(UnityAction ua) => _dialogOk.OnOk += ua;
	public void RemoveOnOk_Ok(UnityAction ua) => _dialogOk.OnOk -= ua;
	
	public void SetActive_YesNo(bool active) => _dialogYesNo.gameObject.SetActive(active);
	public void SetTitle_YesNo(string title) => _dialogYesNo.SetTitle(title);
	public void SetContent_YesNo(string content) => _dialogYesNo.SetContent(content);
	public void AddOnYes_YesNo(UnityAction ua) => _dialogYesNo.OnYes += ua;
	public void RemoveOnYes_YesNo(UnityAction ua) => _dialogYesNo.OnYes -= ua;
	public void AddOnNo_YesNo(UnityAction ua) => _dialogYesNo.OnNo += ua;
	public void RemoveOnNo_YesNo(UnityAction ua) => _dialogYesNo.OnNo -= ua;
}
