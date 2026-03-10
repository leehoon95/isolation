using UnityEngine;

public abstract class PooledEffectBase : MonoBehaviour, IDynamicPooledObject
{
	IPooledDynamicSpawner _spawner;

	public bool IsIllusion { get; set; }
	public string PrefabId { get; set; }
	public string ObjectId { get; set; }
	public ulong OwnerClientId { get; set; }
	public GameObject GO => gameObject;
	public IPooledDynamicSpawner Spawner { get => _spawner; set => _spawner = value; }
	public IDynamicPooledObject DPO => this;
	public bool Play { get; set; }

	public virtual void SetTransform(Vector2 position, Quaternion rotation)
	{
		gameObject.transform.SetPositionAndRotation(position, rotation);
	}

	protected void ReleaseObject()
	{
		Spawner?.ReleaseEffectObject(this);
	}
}
