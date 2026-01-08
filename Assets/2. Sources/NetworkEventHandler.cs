using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkEventHandler : MonoBehaviour
{
	public event Action OnServerStarted;
	public event Action<bool> OnServerStopped;
	public event Action OnClientStarted;
	public event Action<bool> OnClientStopped;
	public event Action<ulong> OnClientConnected;
	public event Action<ulong> OnClientDisconnected;
	public event Action<ulong> OnPeerConnected;
	public event Action<ulong> OnPeerDisconnected;

	void OnEnable()
	{
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
	}

	void OnDisable()
	{
		NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
		NetworkManager.Singleton.OnServerStopped -= OnServerStopped;
		NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
		NetworkManager.Singleton.OnClientStopped -= OnClientStopped;
		NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
		OnServerStarted = null;
		OnServerStopped = null;
		OnClientStarted = null;
		OnClientStopped = null;
		OnClientConnected = null;
		OnClientDisconnected = null;
		OnPeerConnected = null;
		OnPeerDisconnected = null;
	}

	void OnConnectionEvent(NetworkManager nm, ConnectionEventData eventData)
	{
		switch(eventData.EventType)
		{
			case ConnectionEvent.ClientConnected: // This event is set on the client-side of the newly connected client and on the server-side.
				OnClientConnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.ClientDisconnected: // This event is set on the client-side of the client that disconnected client and on the server-side.
				OnClientDisconnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.PeerConnected: // This event is set on clients that are already connected to the session.
				OnPeerConnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.PeerDisconnected: // This event is set on clients that are already connected to the session.
				OnPeerDisconnected?.Invoke(eventData.ClientId);
				break;
		}
	}
}
