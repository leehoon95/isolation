using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/*
 * Rpc메서드로 전달해서 projectile을 설정하기 위한 struct
 */
public struct ProjectileRpcParameter : INetworkSerializable
{
	public float Speed;
	/*
	 * LayerMask -> int: var mask = LayerMask.GetMask("Enemy", "StaticObject"); mask.value
	 * int -> LayerMask: (LayerMask)value
	 */
	public int CollisionMask;
	public int CollisionEffect;
	public FixedString32Bytes CollisionEffectDetail;
	public Color ProjectileColor;
	public float LifeTime;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref Speed);
		serializer.SerializeValue(ref CollisionMask);
		serializer.SerializeValue(ref CollisionEffect);
		serializer.SerializeValue(ref CollisionEffectDetail);
		serializer.SerializeValue(ref ProjectileColor);
		serializer.SerializeValue(ref LifeTime);
	}
}

public struct EffectRpcParameter : INetworkSerializable
{
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		
	}
}