using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct EnemySpawnSpotConfig
{
	public string Name;
	public GameObject Spot;
}

[Serializable]
public struct LevelSwitchConfig
{
	public string Name;
	public LevelSwitch LevelSwitch;
}

public interface StaticObjectHandler
{
	public event UnityAction<string, int> LevelSwitchTriggered;

	public int MaxPlayers { get; set; }
	
	public void OpenDoor(string doorName);
	public Vector2 GetSpawnSpot(string spotGroupName, int index);
	public void SetLevelEndCounter(int newValue, int maxValue);
}

public class StaticObjectManager_Level_0 : NetworkBehaviour, StaticObjectHandler
{
	[SerializeField]
	DoorManager _dm;
	[SerializeField]
	List<EnemySpawnSpotConfig> _enemySpawnSpotConfigs;
	[SerializeField]
	List<LevelSwitchConfig> _levelSwitchConfigs;
	[SerializeField]
	TMP_Text _endCounterText;
	[SerializeField]
	string _levelEndSwitchName;
	[SerializeField]
	LevelSwitch _levelEndSwitch;
	[SerializeField]
	int _maxPlayers;

	UILevelSO _uiso;
	Dictionary<string, Vector2> _enemySpawnSpot = new();
	Dictionary<string, LevelSwitch> _levelSwitch = new();

	public event UnityAction<string, int> LevelSwitchTriggered;

	public int MaxPlayers
	{
		get => _maxPlayers;
		set
		{
			_maxPlayers = value;
			_levelEndSwitch.MaxWeight = value;
			SetLevelEndCounter(0, value);
		}
	}

	public override void OnNetworkSpawn()
	{
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;

		foreach (var config in _enemySpawnSpotConfigs)
		{
			_enemySpawnSpot[config.Name] = config.Spot.transform.position;
		}

		foreach (var config in _levelSwitchConfigs)
		{
			config.LevelSwitch.SwitchName = config.Name;
			config.LevelSwitch.SwitchTriggered += OnSwitchTriggered;
			_levelSwitch[config.Name] = config.LevelSwitch;
		}

		_levelEndSwitch.SwitchName = _levelEndSwitchName;
		_levelEndSwitch.SwitchTriggered += OnSwitchTriggered;
		_endCounterText.text = $"00 / 00";
	}

	public void OnSwitchTriggered(string name, int triggeredCount)
	{
		LevelSwitchTriggered?.Invoke(name, triggeredCount);
		if (name == _levelEndSwitchName)
		{
			SetLevelEndCounter(triggeredCount, MaxPlayers);
		}
	}

	public void OpenDoor(string doorName)
	{
		_dm.OpenDoor(doorName);
	}

	public Vector2 GetSpawnSpot(string spotGroupName, int index)
	{
		var key = $"{spotGroupName}_{index}";
		if (_enemySpawnSpot.TryGetValue(key, out var spot))
		{
			return spot;
		}
		else
		{
			GLogger.LogWarning($"Unknown Spawn Spot. {spotGroupName}");
			return Vector2.zero;
		}
	}

	public void SetLevelEndCounter(int newValue, int maxValue)
	{
		SetLevelEndCounterRpc(newValue, maxValue);
	}

	[Rpc(SendTo.Everyone)]
	void SetLevelEndCounterRpc(int newValue, int maxValue)
	{
		
		_endCounterText.text = $"{newValue:D2} / {maxValue:D2}";
	}
}
