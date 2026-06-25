using System;
using System.Collections;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class NetworkEventHandler : MonoBehaviour
{
	public event Action<ulong> OnClientConnected;
	public event Action<ulong> OnClientDisconnected;
	public event Action<ulong> OnPeerConnected;
	public event Action<ulong> OnPeerDisconnected;
	public event Action<SceneEventType, ulong> OnSceneEvent;

	void Start()
	{
		DontDestroyOnLoad(gameObject);
		NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;
		NetworkManager.Singleton.OnPreShutdown += OnPreShutdown;
	}

	void OnPreShutdown()
	{
		GLogger.LogWarning("OnPreShutdown");
		NetworkManager.Singleton.SceneManager.OnSceneEvent -= SceneEventListner;
		NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;
		NetworkManager.Singleton.OnPreShutdown -= OnPreShutdown;
		ClearConnectionEventListner();
	}

	public void ClearConnectionEventListner()
	{
		OnClientConnected = null;
		OnClientDisconnected = null;
		OnPeerConnected = null;
		OnPeerDisconnected = null;
		OnSceneEvent = null;
	}

	void OnConnectionEvent(NetworkManager nm, ConnectionEventData eventData)
	{
		switch (eventData.EventType)
		{
			case ConnectionEvent.ClientConnected: // This event is set on the client-side of the newly connected client and on the server-side.
				GLogger.LogWarning($"ClientConnected {eventData.ClientId}");

				if (eventData.ClientId == nm.LocalClientId)
				{
					NetworkManager.Singleton.SceneManager.OnSceneEvent += SceneEventListner;
				}
			
				OnClientConnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.ClientDisconnected: // This event is set on the client-side of the client that disconnected client and on the server-side.
				GLogger.LogWarning($"ClientDisconnected {eventData.ClientId}");
				OnClientDisconnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.PeerConnected: // This event is set on clients that are already connected to the session.
				GLogger.LogWarning($"PeerConnected {eventData.ClientId}");
				OnPeerConnected?.Invoke(eventData.ClientId);
				break;
			case ConnectionEvent.PeerDisconnected: // This event is set on clients that are already connected to the session.
				GLogger.LogWarning($"PeerDisconnected {eventData.ClientId}");
				OnPeerDisconnected?.Invoke(eventData.ClientId);
				break;
		}
	}

	void SceneEventListner(SceneEvent sceneEvent) 
		=> OnSceneEvent?.Invoke(sceneEvent.SceneEventType, sceneEvent.ClientId);
}
