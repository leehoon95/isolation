using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public class WeaponConfig
{
	public string Name;
	public Sprite Sprite;
	public string ProjectileName;
	[Tooltip("millisecond")]
	public long FiringInterval;
}

public class WeaponContainer : NetworkBehaviour
{
	[SerializeField]
	List<WeaponConfig> _weaponConfig;
	[SerializeField]
	List<WeaponConfig> _frontWeaponConfig;
	[SerializeField]
	SpriteRenderer _leftSpriteRenderer;
	[SerializeField]
	Transform _leftMuzzle;
	[SerializeField]
	SpriteRenderer _rightSpriteRenderer;
	[SerializeField]
	Transform _rightMuzzle;
	[SerializeField]
	SpriteRenderer _frontSpriteRenderer;
	[SerializeField]
	Transform _frontMuzzle;
	[SerializeField]
	LineRenderer _lineRenderer;
	[SerializeField]
	int _lineSegment = 10;
	[SerializeField]
	float _radius = 2f;

	Dictionary<string, WeaponConfig> _weaponConfigCache = new();
	Dictionary<string, Sprite> _sprites = new();
	IWeaponInterface _leftWeapon;
	IWeaponInterface _rightWeapon;
	IWeaponInterface _frontWeapon;

	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public Vector2 TargetPosition { get; set; }

	void Start()
	{
		foreach (var weapon in _weaponConfig)
		{
			_sprites[weapon.Name] = weapon.Sprite;
			_weaponConfigCache[weapon.Name] = weapon;

		}

		foreach (var weapon in _frontWeaponConfig)
		{
			_sprites[weapon.Name] = weapon.Sprite;
		}

		if (!IsOwner)
		{
			return;
		}

		
		_lineRenderer.useWorldSpace = false;

	}

	public void Trigger(bool on)
	{
		if (_leftWeapon != null)
		{
			_leftWeapon.TargetPosition = TargetPosition;
			_leftWeapon.Trigger(on);
		}

		if (_rightWeapon != null)
		{
			_rightWeapon.TargetPosition = TargetPosition;
			_rightWeapon.Trigger(on);
		}

		if (_frontWeapon != null)
		{
			_frontWeapon.TargetPosition = TargetPosition;
			_frontWeapon.Trigger(on);
		}
	}

	public void SetWeaponSprite(int position, string weaponName)
	{
		switch (position)
		{
			case 0:
				_rightSpriteRenderer.sprite = _sprites[weaponName];
				_rightSpriteRenderer.color = PersonalColor;
				break;
			case 1:
				_frontSpriteRenderer.sprite = _sprites[weaponName];
				_frontSpriteRenderer.color = PersonalColor;
				break;
			case 2:
				_leftSpriteRenderer.sprite = _sprites[weaponName];
				_leftSpriteRenderer.color = PersonalColor;
				break;
		}
	}

	void CreateCircle(float radius)
	{
		float x, y, z = 0;
		float angle = 20f;

		_lineRenderer.positionCount = _lineSegment + 1;

		for (int i = 0; i < (_lineSegment + 1); ++i)
		{
			x = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
			y = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;

			_lineRenderer.SetPosition(i, new Vector3(x, y, z));
			
			angle += (360f / _lineSegment);
		}

	}

	/*
	 * position: 0(right), 1(front), 2(left)
	 */
	[Rpc(SendTo.Everyone)]
	public void SetWeaponRpc(int position, string weaponName)
	{
		if (position < 0 && position > 2)
		{
			GLogger.Log($"Unknown weapon position {position}");
			return;
		}

		if (!_weaponConfigCache.TryGetValue(weaponName, out var config))
		{
			GLogger.Log($"Unknown weapon name {weaponName}");
			return;
		}

		SetWeaponSprite(position, weaponName);

		if (!IsOwner)
		{
			return;
		}

		GLogger.Log($"Set Weapon {weaponName} {position}");

		var iw = GetWeapon(weaponName);
		iw.ProjectileName = config.ProjectileName;
		iw.IPDS = IPDS;
		iw.PersonalColor = PersonalColor;
		iw.FiringInterval = config.FiringInterval;
		iw.ClientId = OwnerClientId;

		if (position == 0)
		{
			iw.Muzzle = _rightMuzzle;
			iw.IsRightWeapon = true;
			_rightWeapon = iw;
		}
		else if (position == 1 )
		{
			iw.Muzzle = _frontMuzzle;
			_frontWeapon = iw;	
		}
		else if (position == 2)
		{
			iw.Muzzle = _leftMuzzle;
			_leftWeapon = iw;
		}

		if (_leftWeapon?.WeaponName == "missile"
			|| _rightWeapon?.WeaponName == "missile")
		{
			CreateCircle(2f);
		}
		else if (_leftWeapon?.WeaponName == "bolt"
			|| _rightWeapon?.WeaponName == "bolt")
		{
			CreateCircle(1f);
		}
		else
		{
			_lineRenderer.positionCount = 0;
		}
	}

	IWeaponInterface GetWeapon(string weaponName)
	{
		switch (weaponName)
		{
			case "bolt": return new WeaponBolt();
			case "missile": return new WeaponMissile();
			case "laser": return new WeaponLaser();
			default: return null;
		}
	}
}
