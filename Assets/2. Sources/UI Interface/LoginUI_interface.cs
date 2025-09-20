using System;
using UnityEngine;
using UnityEngine.Events;

public interface ILoginUI
{
	public void SetNickname(string nickname);
}

public interface ILoginDialogManager
{
	public void SetActive_Ok(bool active);
	public void SetTitle_Ok(string title);
	public void SetContent_Ok(string content);
	public void SetOkButtonText_Ok(string text);
	public void AddOnOk_Ok(UnityAction ua);
	public void RemoveOnOk_Ok(UnityAction ua);
	public void SetActive_YesNo(bool active);
	public void SetTitle_YesNo(string title);
	public void SetContent_YesNo(string content);
	public void AddOnYes_YesNo(UnityAction ua);
	public void RemoveOnYes_YesNo(UnityAction ua);
	public void AddOnNo_YesNo(UnityAction ua);
	public void RemoveOnNo_YesNo(UnityAction ua);
}

