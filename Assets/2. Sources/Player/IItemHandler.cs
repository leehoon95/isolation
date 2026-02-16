using Unity.Netcode;
using UnityEngine;

public interface IItemHandler 
{
    public NetworkObject NO { get; }
	public GameObject GO { get;}
	public ItemType ItemType { get; set; }
	public string ItemEffect { get; set; }
	public bool IsSelected { get; set; }
	public string ItemDescription { get; set; }
	public void Despawn();
}
