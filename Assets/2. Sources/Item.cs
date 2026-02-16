using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;

public enum ItemType
{
	Weapon,
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
	List<Sprite> _weaponIconSprites = new();
	[SerializeField]
	List<Sprite> _buffSprites = new();

	NetworkObject _no;
	bool _isSelected;
//#if UNITY_EDITOR
//	void OnValidate()
//	{
//		EditorApplication.delayCall += () =>
//		{
//			SetItemShapeForType(_type);
//		};
//	}
//#endif

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
				_selectedBackgroundSprite.gameObject.SetActive(true);
			}
			else
			{
				_selectedBackgroundSprite.gameObject.SetActive(false);
			}
		}
	}

	public string ItemDescription 
	{
		get; set;
	}

	void Start()
	{
		
	}

	public void Despawn()
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

	public CollisionEffect GetEffect()
	{
		throw new System.NotImplementedException();
	}
}
