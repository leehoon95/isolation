using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

/*
 * Rpc메서드로 전달해서 projectile을 설정하기 위한 struct
 */
public struct ProjectileRpcParameter : INetworkSerializable
{
	public Vector2 StartPosition;
	public Vector2 TartgetPosition;
	//public float Speed;
	//public float SpeedDeltaPerSec; // 가감속을 위한 값
	//public float MaxAngularVelocity; // 최대 각속도(flying type이 Curve일 때만 유효)
	/*
	 * LayerMask -> int: var mask = LayerMask.GetMask("Enemy", "StaticObject"); mask.value
	 * int -> LayerMask: (LayerMask)value
	 */
	//public int CollisionIncludeLayers;
	//public int CollisionExcludeLayers;
	public CollisionEventStruct CollisionEvent;
	public Color EffectColor;
	public float LifeTime;
	

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref StartPosition);
		serializer.SerializeValue(ref TartgetPosition);
		//serializer.SerializeValue(ref Speed);
		//serializer.SerializeValue(ref SpeedDeltaPerSec);
		//serializer.SerializeValue(ref MaxAngularVelocity);
		//serializer.SerializeValue(ref CollisionIncludeLayers);
		//serializer.SerializeValue(ref CollisionExcludeLayers);
		CollisionEvent.NetworkSerialize(serializer);
		serializer.SerializeValue(ref EffectColor);
		serializer.SerializeValue(ref LifeTime);
	}
}

public struct EffectRpcParameter : INetworkSerializable
{
	public Color EffectColor;
	public float Data1;
	public float Data2;
	//public float Data3;
	//public float Data4;

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref EffectColor);
		serializer.SerializeValue(ref Data1);
		serializer.SerializeValue(ref Data2);
		//serializer.SerializeValue(ref Data3);
		//serializer.SerializeValue(ref Data4);
	}
}