using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IEnemySpawnEventRaiserble
{
	public void RaiseEnemySpawned(string prefabId);
	public void RaiseEnemyDespawned(string prefabId);
}

public interface IPlayerSpawnEventRaiserble
{
	public void RaisePlayerSpawned(ulong clientId);
	public void RaisePlayerDespawned(ulong clientId);
}

public interface IStaticPadEventRaiserble
{
	public void RaisePlayerStandOnPad(int count);
}

// deprecated
public class EventServiceLocator : MonoBehaviour, IEnemySpawnEventRaiserble, IPlayerSpawnEventRaiserble, IStaticPadEventRaiserble
{
	public event UnityAction<string> OnEnemySpawned;
	public event UnityAction<string> OnEnemyDespawned;
	public event UnityAction<ulong> OnPlayerSpawned;
	public event UnityAction<ulong> OnPlayerDead;
	public event UnityAction<int> OnPlayerStandOnPad;

	Dictionary<string, UnityEvent> _events = new();

	public void RaiseEnemySpawned(string prefabId)
		=> OnEnemySpawned?.Invoke(prefabId);
	public void RaiseEnemyDespawned(string prefabId)
		=> OnEnemyDespawned?.Invoke(prefabId);
	public void RaisePlayerSpawned(ulong clientId)
		=> OnPlayerSpawned?.Invoke(clientId);
	public void RaisePlayerDespawned(ulong clientId)
		=> OnPlayerDead?.Invoke(clientId);
	public void RaisePlayerStandOnPad(int count)
		=> OnPlayerStandOnPad?.Invoke(count);
}
