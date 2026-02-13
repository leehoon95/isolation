using UnityEngine;


public interface IWeaponInterface
{
	public Transform Transform { get; }
	public Transform MuzzleTransform { get; }

	public PooledDynamicSpawner PDS { get; set; }
	public Color PersonalColor { get; set; }
	public int Round {  get; set; }
	/*
	 *  < 0: completed
	 *  > 0: charging
	 */
	public float ChargingTime { get; set; }
	public bool Trigger(bool on);
	public bool SetEvent(string eventName, float time);
}
