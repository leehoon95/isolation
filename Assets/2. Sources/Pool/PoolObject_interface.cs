using Unity.Netcode;
using UnityEngine;

/*
 * Pool에 들어가는 object interface
 */
public interface IDynamicPooledObject
{
	bool IsIllusion { get; set; }
	string PrefabId { get; set; }
	string ObjectId { get; set; }
	ulong OwnerClientId { get; set; }
	GameObject GO { get; }
	IPooledDynamicSpawner Spawner { set; }
	IDynamicPooledObject DPO { get; }
	void SetTransform(Vector2 position, Quaternion rotation);
}

/*
 * Pooled item에서 pool을 참조 목적용 
 */
public interface IPooledDynamicSpawner
{
	void ReleaseObject(IDynamicPooledObject dpo);
	void ReleaseEffectObject(IDynamicPooledObject dpo);
}