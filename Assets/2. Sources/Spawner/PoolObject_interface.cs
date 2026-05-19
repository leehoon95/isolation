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
	IPooledDynamicSpawner IPDS { set; }
	IDynamicPooledObject IDPO { get; }
	bool Play { get; set; }
	void SetTransform(Vector2 position, Quaternion rotation);
}

/*
 * Pooled item에서 pool을 참조 목적용 
 */
public interface IPooledDynamicSpawner
{
	public void CreateProjectile(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in ProjectileRpcParameter prp);
	public void CreateEffect(
		string prefabId,
		Vector2 position,
		Quaternion rotation,
		in EffectRpcParameter erp,
		bool reliable = true);
	public void CreateEffectLocal(
	string prefabId,
	Vector2 position,
	Quaternion rotation,
	in EffectRpcParameter erp);
	void ReleaseObject(IDynamicPooledObject dpo);
	void ReleaseEffectObject(IDynamicPooledObject dpo);
}