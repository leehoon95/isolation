using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public enum ItemType
{
	Weapon,
	AutomaticWeapon,
	Buff,

}

public class Item : NetworkBehaviour, ICollisionInteractable
{
	[SerializeField]
	SpriteRenderer _itemSprite;
	[SerializeField]
	SpriteRenderer _backgroundSprite;
	[SerializeField]
	ItemType _type;
	[SerializeField]
	string _effect;
	[SerializeField]
	List<Sprite> _weaponIconSprites = new();
	[SerializeField]
	List<Sprite> _automaticWeaponIconSprites = new();
	[SerializeField]
	List<Sprite> _buffSprites = new();

#if UNITY_EDITOR
	void OnValidate()
	{
		EditorApplication.delayCall += () =>
		{
			SetItemShapeForType(_type);
		};
	}
#endif

	public ItemType ItemType { get; set; }
	public string ItemEffect { get; set; }
	public string GetEffect()
	{
		return _effect;
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		
	}

	void SetItemShapeForType(ItemType type)
	{
		if (type == ItemType.Weapon)
		{
			_backgroundSprite.color = new Color(22f / 255f, 147f / 255f, 146f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				case "homing": s = _weaponIconSprites[0]; break;
				case "cluster": s = _weaponIconSprites[1]; break;
				case "laser": s = _weaponIconSprites[2]; break;
				case "bolt": s = _weaponIconSprites[3]; break;
				default: s = null; break;
			}

			_itemSprite.sprite = s;
		}
		else if (type == ItemType.AutomaticWeapon)
		{
			_backgroundSprite.color = new Color(1f, 64f / 255f, 146f / 255f);
			Sprite s = null;
			switch (_effect)
			{
				case "shield": s = _automaticWeaponIconSprites[0]; break;
				case "shock": s = _automaticWeaponIconSprites[1]; break;
				case "wave": s = _automaticWeaponIconSprites[2]; break;
				default: s = null; break;
			}

			_itemSprite.sprite = s;
		}
		else if (type == ItemType.Buff)
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

	public void AddCollisionEvent(CollisionEvent ce)
	{
		throw new System.NotImplementedException();
	}

	CollisionEffect ICollisionInteractable.GetEffect()
	{
		return CollisionEffect.Item;
	}
}
