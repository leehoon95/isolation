using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerEventSO", menuName = "Scriptable Objects/ServerEventSO")]
public class ServerEventSO : ScriptableObject
{
	public event Action<string> OnServerConnected;
	public event Action<string> OnServerDisconnected;
	public event Action<string> OnServerMessageReceived;
	public event Action<string> OnServerMessageSent;

	public async void RaiseServerConnected(string message)
	{
		await Awaitable.MainThreadAsync();
		OnServerConnected?.Invoke(message);
	}
	public async void RaiseServerDisconnected(string message)
	{
		await Awaitable.MainThreadAsync();
		OnServerDisconnected?.Invoke(message);

	}
	public async void RaiseServerMessageReceived(string message)
	{
		await Awaitable.MainThreadAsync();
		OnServerMessageReceived?.Invoke(message);

	}
	public async void RaiseServerMessageSent(string message)
	{
		await Awaitable.MainThreadAsync();
		OnServerMessageSent?.Invoke(message);
	}
}
