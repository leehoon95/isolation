using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public enum ItemType : int
{
	Weapon = 0,
	Buff,

}

public class Item : NetworkBehaviour, IItemHandler
{
	[SerializeField]
	SpriteRenderer _itemSprite;
	[SerializeField]
	SpriteRenderer _backgroundSprite;
	[SerializeField]
	SpriteRenderer _selectedBackgroundSprite;
	[SerializeField]
	ItemType _type;
	[SerializeField]
	string _effect;
	[SerializeField]
	SortingGroup _sortingGroup;
	[SerializeField]
	List<Sprite> _weaponIconSprites = new();
	[SerializeField]
	List<Sprite> _buffSprites = new();

	NetworkObject _no;
	bool _isSelected;
	DateTime _spawnedTime;
	Coroutine _timeoutCo;
	WaitForSeconds _wait = new WaitForSeconds(10f);

	public NetworkObject NO
	{
		get => GetComponent<NetworkObject>();
	}
	public GameObject GO
	{
		get => gameObject;
	}
	public ItemType ItemType
	{
		get => _type;
		set => _type = value;
	}
	public string ItemEffect
	{
		get => _effect;
		set => _effect = value;
	}
	public bool IsSelected
	{
		get => _isSelected;
		set
		{
			_isSelected = value;
			if (_isSelected)
			{
				_sortingGroup.sortingOrder = 1;
				_selectedBackgroundSprite.gameObject.SetActive(true);
			}
			else
			{
				_sortingGroup.sortingOrder = 0;
				_selectedBackgroundSprite.gameObject.SetActive(false);
			}
		}
	}

	public bool IsOnlyFront
	{
		get
		{
			if (_effect == "shield"
				|| _effect == "shock"
				|| _effect == "wave")
			{
				return true;
			}

			return false;
		}
	}

	public DateTime SpawnedTime => _spawnedTime;

	void OnEnable()
	{
		_sortingGroup.sortingOrder = 0;
		IsSelected = false;
		if (IsHost)
		{
			_spawnedTime = DateTime.Now;
			_timeoutCo = StartCoroutine(ReleaseTimeout());
		}
	}

	void OnDisable()
	{
		if (IsHost && _timeoutCo != null)
		{
			StopCoroutine(_timeoutCo);
		}
	}

	IEnumerator ReleaseTimeout()
	{
		yield return _wait;

		_timeoutCo = null;
		DespawnItemRpc();
	}

	[Rpc(SendTo.Server)]
	public void DespawnItemRpc()
	{
		NetworkObject.Despawn();
	}

	public void RefreshItemShape()
	{
		if (_type == ItemType.Weapon)
		{
			_backgroundSprite.color = new Color(22f / 255f, 147f / 255f, 146f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				case "shield": s = _weaponIconSprites[0]; break;
				case "shock": s = _weaponIconSprites[1]; break;
				case "homing": s = _weaponIconSprites[2]; break;
				case "cluster": s = _weaponIconSprites[3]; break;
				case "wave": s = _weaponIconSprites[4]; break;
				case "laser": s = _weaponIconSprites[5]; break;
				case "bolt": s = _weaponIconSprites[6]; break;
				default: s = null; break;
			}

			_itemSprite.sprite = s;
		}
		else if (_type == ItemType.Buff)
		{
			_backgroundSprite.color = new Color(60f / 255f, 179f / 255f, 161f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				case "burst": s = _buffSprites[0]; break;
				case "healing": s = _buffSprites[1]; break;
				case "bomb": s = _buffSprites[2]; break;
				default: s = null; break;
			}
			_itemSprite.sprite = s;
		}
	}
}
