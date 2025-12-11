using System;
using UnityEngine;

public class InterfaceTypeAttribute : PropertyAttribute
{
	public Type InterfaceType { get; private set; }
	public InterfaceTypeAttribute(Type interfaceType)
	{
		InterfaceType = interfaceType;
	}
}

public interface INotificationUI
{
	public void ShowNotification(string content);
}

public interface ISupportNotificationUI
{
	public INotificationUI Notification { get; set; }
	public void ShowNotification(string text);
}

