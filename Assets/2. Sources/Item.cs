using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;

public enum ItemType : int
{
	Weapon = 0,
	FrontWeapon,
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
	Collider2D _collider;
	[SerializeField]
	List<Sprite> _weaponIconSprites;
	[SerializeField]
	List<Sprite> _frontWeaponSprites;
	[SerializeField]
	List<Sprite> _buffSprites;

	bool _isSelected;
	WaitForSeconds _wait = new WaitForSeconds(15f);

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
	}
	public string ItemEffect
	{
		get => _effect;
		set
		{
			_effect = value;
			switch (value)
			{
				case "bolt":
				case "missile":
				case "laser":
					_type = ItemType.Weapon;
					break;
				case "shock":
					_type = ItemType.FrontWeapon;
					break;
				case "shield":
				case "burst":
				case "heal":
				case "bomb":
					_type = ItemType.Buff;
					break;
				default:
					_effect = "";
					break;
			}

			if (_type == ItemType.Buff)
			{
				_collider.includeLayers = 1 << LayerMask.NameToLayer("Player Body");
				_collider.excludeLayers = 1 << LayerMask.NameToLayer("Player Hand");
			}
			else
			{
				_collider.excludeLayers = 0;
				_collider.includeLayers = 0;
			}
		}
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

	public override void OnNetworkSpawn()
	{
		if (IsHost)
		{
			StartCoroutine(ReleaseTimeout());
		}
	}

	void OnEnable()
	{
		_sortingGroup.sortingOrder = 0;
		IsSelected = false;
		
	}

	void OnDisable()
	{
		if (IsHost )
		{
			StopAllCoroutines();
		}
	}

	IEnumerator ReleaseTimeout()
	{
		yield return _wait;
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
				case "missile": s = _weaponIconSprites[0]; break;
				case "laser": s = _weaponIconSprites[1]; break;
				case "bolt": s = _weaponIconSprites[2]; break;
				default: s = null; break;
			}

			_itemSprite.sprite = s;
		}
		else if (_type == ItemType.FrontWeapon)
		{
			_backgroundSprite.color = new Color(14f / 255f, 180f / 255f, 252f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				
				case "shock": s = _frontWeaponSprites[1]; break;
				default: s = null; break;
			}

			_itemSprite.sprite = s;
		}
		else if (_type == ItemType.Buff)
		{
			_backgroundSprite.color = new Color(237f / 255f, 84f / 255f, 74f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				case "burst": s = _buffSprites[0]; break;
				case "heal": s = _buffSprites[1]; break;
				case "bomb": s = _buffSprites[2]; break;
				case "shield": s = _buffSprites[3]; break;
				default: s = null; break;
			}
			_itemSprite.sprite = s;
		}
	}
}
