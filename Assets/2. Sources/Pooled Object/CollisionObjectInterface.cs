using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Localization.Settings;

/*
 * None : 운동하지 않음
 * Direct : 등속 직선 운동
 * Registed : 감속 직선 운동
 * Homing : 추적으로 인한 회전, 가감속 운동
 */
public enum ProjectileFlyingType : int
{
	None,
	Rectilinear,
	Curve,
	Homing
}

/*
 * 충돌 후 반대편에게 줄 수 있는 물리 효과
 */
public enum CollisionEffect : int
{
	None = 0,
	Knockback,
	Stopping,
	Pop,
	Block
}

public struct CollisionEventStruct : INetworkSerializable
{
	public ulong SenderId;
	/*
	 * Position과 Direction은 충돌시 결정한다
	 */
	public Vector2 Position; // 충돌한 object의 위치
	public Vector2 Direction; // 충돌한 object의 이동 방향(힘의 방향)
	public CollisionEffect Effect; // 효과
	public float EffectIntensity;
	public float EffectDuration; // 효과 지속시간
	public int Damage; // 대미지

	public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
	{
		serializer.SerializeValue(ref SenderId);
		serializer.SerializeValue(ref Position);
		serializer.SerializeValue(ref Direction);
		serializer.SerializeValue(ref Effect);
		serializer.SerializeValue(ref EffectIntensity);
		serializer.SerializeValue(ref EffectDuration);
		serializer.SerializeValue(ref Damage);
	}

	public static implicit operator CollisionEventStruct(CollisionEvent ce)
	{
		return new()
		{
			SenderId = ce.SenderId,
			Position = ce.Position,
			Direction = ce.Direction,
			Effect = ce.Effect,
			EffectIntensity = ce.EffectIntensity,
			EffectDuration = ce.EffectDuration,
			Damage = ce.Damage,
		};
	}
}

public class CollisionEvent
{
	public ulong SenderId;
	public Vector2 Position;
	public Vector2 Direction;
	public CollisionEffect Effect;
	public float EffectIntensity;
	public float EffectDuration;
	public int Damage;

	public CollisionEvent FromCollisionEventStruct(in CollisionEventStruct ce)
	{
		SenderId = ce.SenderId;
		Position = ce.Position;
		Direction = ce.Direction;
		Effect = ce.Effect;
		EffectIntensity = ce.EffectIntensity;
		EffectDuration = ce.EffectDuration;
		Damage = ce.Damage;

		return this;
	}
}

/*
 * INetworkObjectCollision 인터페이스를 구현하는 오브젝트에게 충돌 이벤트를 전달할 수 있다
 * 충돌검사는 local scene에서 판정하되, 정확한 이벤트 전달을 위해(충돌 이벤트가 중복되는 것을 방지)
 * host가 local object에게 충돌 이벤트를 전달한다.
 * 충돌 이벤트는 NetworkManager에서 알 수 있는 Spawn된 오브젝트여야 한다
 */
public interface INetworkObjectCollision
{
	public void SendCollisionEvent(CollisionEvent ce);
	public CollisionEvent GetCollisionEvent();
}
