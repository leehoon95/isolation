using UnityEngine;


public interface IWeaponInterface
{
	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName { get; }
	public GameObject GO { get; }
	

	public void Trigger(bool on);
	public void ApplyBuff(string buffName);
	public void RemoveBuff(string buffName);
	public void Stop();
}
