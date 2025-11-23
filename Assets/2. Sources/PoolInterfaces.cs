using Unity.Netcode;
using UnityEngine;

/*
 * Pool에 들어가는 object interface
 * PrefabId: 어떤 prefab에서 Instantiate되었는 알 수 있는 값
 * ObjectId: (client id)_(local에서 중복되지 않는 값) => 로컬, 네트워크에서 중복되지 않음
 * SO: prefab 생성시 연결할 ScriptableObject. 생성된 instance는 이 SO를 변경하면 안 됨.
 */
public interface IDynamicPooledObject
{
	string PrefabId { get; set; }
	string ObjectId { get; set; }
	ulong ClientId { get; set; }
	ScriptableObject SO { get; set; }
	IPooledDynamicSpawner Spawner { set; }
	IDynamicPooledObject DPO { get; }
	GameObject GO { get; }
	NetworkObject NO { get; }
	bool OnlyInteractInOwnerClient { get; set; }
	void SetLifeTime(bool active, float time = 0f);
	void SetTransform(Vector2 position, Quaternion rotation);
	void Clean();
}

/*
 * Pooled item에서 pool을 참조 목적용 
 */
public interface IPooledDynamicSpawner
{
	void ReleaseObject(IDynamicPooledObject obj);
	//void Despawn(IDynamicPooledObject go);
}