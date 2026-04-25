using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LevelSwitch : MonoBehaviour
{
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	string _name;
	[SerializeField]
	bool _weightSwitch;

	HashSet<ulong> _idSet = new();

	public UnityAction<string, int> SwitchTriggered;
	
	public string SwitchName
	{
		get => _name;
		set => _name = value;
	}
	public int MaxWeight { get; set; }

	void OnTriggerEnter2D(Collider2D collision)
	{
		var ph = collision.GetComponentInParent<IPlayerHandler>();
		if (ph != null)
		{
			_idSet.Add(ph.SpawnClientId);
			SwitchTriggered?.Invoke(_name, _idSet.Count);
		}
	}

	void OnTriggerExit2D(Collider2D collision)
	{
		var ph = collision.GetComponentInParent<IPlayerHandler>();
		if (ph != null)
		{
			//GLogger.Log($"LV Switch {_name} exit");
			_idSet.Remove(ph.SpawnClientId);
			SwitchTriggered?.Invoke(_name, _idSet.Count);
		}
	}
}
