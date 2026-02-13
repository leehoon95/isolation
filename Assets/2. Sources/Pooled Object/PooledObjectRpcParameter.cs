using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/*
 * Rpc메서드로 전달해서 projectile을 설정하기 위한 struct
 */
public struct ProjectileRpcParameter : INetworkSerializable
{
	public ProjectileFlyingType FlyingType;
	public Vector2 StartPosition;
	public Vector2 TartgetPosition;
	public float Speed;
	public float SpeedDeltaPerSec; // 가감속을 위한 값
	public float MaxAngularVelocity; // 최대 각속도(flying type이 Curve일 때만 유효)
	/*
	 * LayerMask -> int: var mask = LayerMask.GetMask("Enemy", "StaticObject"); mask.value
	 * int -> LayerMask: (LayerMask)value
	 */
	//public int CollisionIncludeLayers;
	//public int CollisionExcludeLayers;
	public int CollisionEffect;
	public FixedString32Bytes CollisionEffectDetail;
	public Color EffectColor;
	public float LifeTime;
	

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref FlyingType);
		serializer.SerializeValue(ref StartPosition);
		serializer.SerializeValue(ref TartgetPosition);
		serializer.SerializeValue(ref Speed);
		serializer.SerializeValue(ref SpeedDeltaPerSec);
		serializer.SerializeValue(ref MaxAngularVelocity);
		//serializer.SerializeValue(ref CollisionIncludeLayers);
		//serializer.SerializeValue(ref CollisionExcludeLayers);
		serializer.SerializeValue(ref CollisionEffect);
		serializer.SerializeValue(ref CollisionEffectDetail);
		serializer.SerializeValue(ref EffectColor);
		serializer.SerializeValue(ref LifeTime);
	}
}

public struct EffectRpcParameter : INetworkSerializable
{
	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		
	}
}