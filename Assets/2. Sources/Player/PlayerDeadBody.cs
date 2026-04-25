using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerDeadBody : NetworkBehaviour, IPlayerDeadBodyHandler
{
	[SerializeField]
	Transform _text;
	[SerializeField]
	TMP_Text _nicknameText;
	[SerializeField]
	ReviveIndicator _indicator;
	[SerializeField]
	float _reviveTime;

	string _nickname;
	Color _personalColor;
	Coroutine _taskCo;
	Dictionary<ulong, IPlayerHandler> _revivingPlayers = new();

	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IPlayerSpawner Spawner { get; set; }
	public string Nickname
	{
		get => _nickname;
		set
		{
			_nickname = value;
			_nicknameText.text = value;
		}
	}
	public Color PersonalColor
	{
		get => _personalColor;
		set
		{
			_personalColor = value;
			_nicknameText.color = value;
		}
	}

	public ulong ClientIdForRevive { get; set; }

	public override void OnNetworkSpawn()
	{
		_text.rotation = Quaternion.identity;
		_indicator.transform.rotation = Quaternion.identity;

		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
			_taskCo = null;
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		GLogger.Log("OnTriggerEnter2D");
		var player = collision.GetComponentInParent<IPlayerHandler>();
		if (player != null)
		{
			_revivingPlayers[player.NO.OwnerClientId] = player;
			if (_taskCo == null)
			{
				var now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
				StartRevivingRpc(now);
			}
		}
	}

	void OnTriggerExit2D(Collider2D collision)
	{
		var player = collision.GetComponentInParent<IPlayerHandler>();
		if (player != null)
		{
			_revivingPlayers.Remove(player.NO.OwnerClientId);
			if (_revivingPlayers.Count == 0)
			{
				StopRevivingRpc();
			}
		}
	}

	[Rpc(SendTo.Everyone)]
	void StartRevivingRpc(long startTick)
	{
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
		}
		_taskCo = StartCoroutine(RevivingProcess(startTick));
	}

	[Rpc(SendTo.Everyone)]
	void StopRevivingRpc()
	{
		if (_taskCo != null)
		{
			StopCoroutine(_taskCo);
			_taskCo = null;
			_indicator.Progress = 0f;
		}
	}

	IEnumerator RevivingProcess(long startTick)
	{
		yield return null;
		GLogger.Log("Start reviving");
		var t = _reviveTime;
		while (t > 0f)
		{
			_indicator.Progress = 1 - t;
			t -= Time.deltaTime;
			yield return null;
		}

		_indicator.Progress = 1f; 

		if (IsHost)
		{
			GLogger.Log($"Revive {ClientIdForRevive} {transform.position}");
			Spawner.SpawnPlayerRpc(
				ClientIdForRevive,
				transform.position,
				Quaternion.identity,
				new PlayerInstantiateData()
				{
					OwnerClientId = ClientIdForRevive,
					Position = transform.position,
					Nickname = _nickname,
					PersonalColor = _personalColor,
				});
			_revivingPlayers.Clear();
			_taskCo = null;
			NO.Despawn();
		}
	}
}
