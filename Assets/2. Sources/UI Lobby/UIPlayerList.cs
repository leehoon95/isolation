using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class UIPlayerList : UIBehaviour, IUILobbyPlayerList
{
	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.PlayerList = this;
	}

	public void AddPlayer(string playerName, Color color)
	{
		throw new System.NotImplementedException();
	}

	public void RemovePlayer(string playerName)
	{
		throw new System.NotImplementedException();
	}
}
