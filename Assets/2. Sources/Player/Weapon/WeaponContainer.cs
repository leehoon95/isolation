using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class WeaponConfig
{
	public string Name;
	public Sprite Sprite;
	public GameObject Prefab;
}

public interface ILaserFiringRpc
{
	public void FireLaserFromOtherClinent(bool isRightWeapon, LaserPointData[] lpd, FixedString32Bytes buffName);
}

public class WeaponContainer : NetworkBehaviour, ILaserFiringRpc
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

	Dictionary<string, WeaponConfig> _weaponConfigCache = new();
	Dictionary<string, Sprite> _sprites = new();
	//Dictionary<string, IWeaponInterface> _weaponCache = new();
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
			_weaponConfigCache[weapon.Name] = weapon;
		}

		if (!IsOwner)
		{
			return;
		}

		_lineRenderer.useWorldSpace = false;

	}

	public void Trigger(bool on)
	{
		if (!IsOwner)
		{
			return;
		}

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

	void SetWeaponSprite(int position, string weaponName)
	{
		//GLogger.Log($"SetWeaponSprite {position} {weaponName}");
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

		if (weaponName == "laser")
		{
			if (position == 0)
			{
				weaponName = "laserRight";
			}
			else if (position == 2)
			{
				weaponName = "laserLeft";
			}
		}

		SetWeaponSprite(position, weaponName);


		WeaponConfig wc;
		_weaponConfigCache.TryGetValue(weaponName, out wc);

		if (wc == null)
		{
			GLogger.LogWarning($"Unknown Weapon {weaponName}");
			return;
		}

		IWeaponInterface wi = null;
		var obj = Instantiate(wc.Prefab);
		if (weaponName.Contains("laser"))
		{
			var wl = obj.GetComponent<WeaponLaser>();
			wl.WCR = this;
		}
		
		wi = obj.GetComponent<IWeaponInterface>();
		wi.IPDS = IPDS;
		wi.PersonalColor = PersonalColor;
		wi.ClientId = OwnerClientId;
		wi.GO.transform.SetParent(transform);
		wi.GO.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

		if (position == 0)
		{
			wi.Muzzle = _rightMuzzle;
			wi.IsRightWeapon = true;
			if (_rightWeapon != null)
			{
				Destroy(_rightWeapon.GO);
			}

			_rightWeapon = wi;
		}
		else if (position == 1)
		{
			wi.Muzzle = _frontMuzzle;
			if (_frontWeapon != null)
			{
				Destroy(_frontWeapon.GO);
			}
			_frontWeapon = wi;
		}
		else if (position == 2)
		{
			wi.Muzzle = _leftMuzzle;
			if (_leftWeapon != null)
			{
				Destroy(_leftWeapon.GO);
			}
			_leftWeapon = wi;
		}

		if (!IsOwner)
		{
			return;
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

	// laser weapon만 사용가능함
	public void FireLaserFromOtherClinent(bool isRightWeapon, LaserPointData[] lpd, FixedString32Bytes buffName)
	{
		ShowLaserStreamRpc(isRightWeapon, lpd, buffName);
	}

	[Rpc(SendTo.NotMe)]
	void ShowLaserStreamRpc(bool isRightWeapon, LaserPointData[] lpd, FixedString32Bytes buffName)
	{
		IWeaponInterface wi;

		if (isRightWeapon)
		{
			wi = _rightWeapon;
		}
		else
		{
			wi = _leftWeapon;
		}

		var wl = wi.GO.GetComponent<WeaponLaser>();
		wl.FireLaserStream(lpd, buffName.ToString());
	}

	public void ApplyBuff(string effect)
	{
		_leftWeapon?.ApplyBuff(effect);
		_rightWeapon?.ApplyBuff(effect);
	}

	public void RemoveBuff()
	{
		_leftWeapon?.RemoveBuff("");
		_rightWeapon?.RemoveBuff("");
	}

	public void Stop()
	{
		_leftWeapon?.Stop();
		_rightWeapon?.Stop();
	}

	IWeaponInterface GetWeapon(string weaponName, int position)
	{
		switch (weaponName)
		{
			case "bolt": return new WeaponBolt();
			case "missile": return new WeaponMissile();
			case "laser":
				if (position == 0)
				{
					return new WeaponLaser();
				}
				else if (position == 2)
				{
					return new WeaponLaser();
				}

				return null;
			default: return null;
		}
	}
}
