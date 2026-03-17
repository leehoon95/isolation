using UnityEngine;


public interface IWeaponInterface
{
	public string ProjectileName { get; set; }
	public IPooledDynamicSpawner IPDS { get; set; }
	public Color PersonalColor { get; set; }
	public bool IsRightWeapon { get; set; }
	public Vector2 TargetPosition { get; set; }
	public long FiringInterval { get; set; }
	public Transform Muzzle { get; set; }
	public ulong ClientId { get; set; }
	public string WeaponName { get; }
	

	public void Trigger(bool on);
	public void AddBuff(string buffName, float time);
}
