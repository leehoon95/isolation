using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(fileName = "ServerEvent", menuName = "Scriptable Objects/ServerEvent")]
public class ServerEventSO : ScriptableObject
{
    public event Action<string> OnServerConnected;
	public event Action<string> OnServerDisconnected;
	public event Action<string> OnServerMessageReceived;
	public event Action<string> OnServerMessageSent;

	public void RaiseServerConnected(string message)
	{
		MainThreadDispatcher.Enqueue(() =>
		{
			OnServerConnected?.Invoke(message);
		});
	}
	public void RaiseServerDisconnected(string message)
	{
		MainThreadDispatcher.Enqueue(() =>
		{
			OnServerDisconnected?.Invoke(message);
		});
	}
	public void RaiseServerMessageReceived(string message)
	{
		MainThreadDispatcher.Enqueue(() =>
		{
			OnServerMessageReceived?.Invoke(message);
		});
	}
	public void RaiseServerMessageSent(string message)
	{
		MainThreadDispatcher.Enqueue(() =>
		{
			OnServerMessageSent?.Invoke(message);
		});
	}
}
