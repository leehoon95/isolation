using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class ChatBoxSynchronizer : NetworkBehaviour
{
	public event Action<string, string, Color> OnReceivedChatMessage;

	[Rpc(SendTo.Server)]
	public void ChatMessageRpc(FixedString64Bytes speaker, FixedString128Bytes text, Color color)
	{
		BroadcastChatMessageRpc(speaker, text, color);
	}

	[Rpc(SendTo.Everyone)]
	void BroadcastChatMessageRpc(FixedString64Bytes speaker, FixedString128Bytes text, Color color)
	{
		OnReceivedChatMessage?.Invoke(speaker.ToString(), text.ToString(), color);
	}
}
