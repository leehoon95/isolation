using System;
using Unity.Netcode;
using UnityEngine;

public interface IItemHandler 
{
    public NetworkObject NO { get; }
	public GameObject GO { get;}
	public ItemType ItemType { get; set; }
	public string ItemEffect { get; set; }
	public DateTime SpawnedTime { get; }
	public bool IsOnlyFront { get; }
	public bool IsSelected { get; set; }
	public void RefreshItemShape();
	public void DespawnItemRpc();
}
