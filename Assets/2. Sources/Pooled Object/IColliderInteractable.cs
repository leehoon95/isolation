using System;
using UnityEngine;

public enum CollisionEffect : int
{
	None = 0,
	Damage,
	DamageAndDebuff,
	Debuff,
}

public class CollisionEvent
{
	public Vector2 From; // 충돌한 object의 위치
	public Vector2 To; // 충돌한 object의 이동 방향
	public CollisionEffect Effect;
	public string EffectDetail;
}

/*
 * collider를 통한 상호작용을 위한 interface
 * 충돌시 바로 이벤트를 실행하는 것이 아니라 object 내부 이벤트 큐에 등록하여
 * object 스스로 처리할 수 있도록 할 것
 */
public interface IColliderInteractable
{
	void AddCollisionEvent(CollisionEvent ce);
	CollisionEffect GetEffect();
	void SetProjectileParameter(in ProjectileRpcParameter param);
}
