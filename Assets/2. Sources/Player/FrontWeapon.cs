using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class FrontWeaponConfig
{
	public string WeaponName;
	public Sprite Sprite;
	public long FiringInterval;
}


public class FrontWeapon : MonoBehaviour
{
	[SerializeField]
	List<FrontWeaponConfig> _config;
}
