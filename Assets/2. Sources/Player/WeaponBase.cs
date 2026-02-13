using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization.Events;

public abstract class WeaponBase : MonoBehaviour
{
	public float FiringInterval { get; protected set; }
	public Vector2 MuzzlePosition { get; protected set; }
	public PooledDynamicSpawner PDS { get; set; }
	public Color PersonalColor { get; set; }
	public abstract bool Trigger(bool on);
	public abstract bool SetEvent(string eventName, float time);
}