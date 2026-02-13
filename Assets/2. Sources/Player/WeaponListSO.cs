using UnityEngine;

[CreateAssetMenu(fileName = "WeaponListSO", menuName = "Scriptable Objects/WeaponListSO")]
public class WeaponListSO : ScriptableObject
{
	public WeaponBolt Pistol;



	public IWeaponInterface GetWeapon(int index)
	{
		switch (index)
		{
			case 0: return Pistol;
			//...
			default: return null;
		}
	}
}
