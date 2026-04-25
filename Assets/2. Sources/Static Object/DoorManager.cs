using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public struct DoorConfig
{
	public string DoorName;
	public GameObject Door;
}

[RequireComponent(typeof(NetworkObject))]
public class DoorManager : NetworkBehaviour
{
	[SerializeField]
	List<DoorConfig> _doorConfigs;

	Dictionary<string, IDoorHandler> _doors = new();
	Coroutine _taskCo;

	void Start()
	{
		foreach (var dc in _doorConfigs)
		{
			var idh = dc.Door.GetComponent<IDoorHandler>();
			if (idh != null)
			{
				_doors[dc.DoorName] = idh;
			}
		}
	}

	public void OpenDoor(string key)
	{
		if (!IsHost || key == null)
		{
			return;
		}

		OpenDoorRpc(key);
	}


	[Rpc(SendTo.Everyone)]
	void OpenDoorRpc(FixedString32Bytes key)
	{
		if (_doors.TryGetValue(key.ToString(), out var idh))
		{
			idh.Open();
		}
		else
		{
			GLogger.LogWarning($"OpenDoorRpc Invalid key. {key.ToString()}");
		}
	}
}
