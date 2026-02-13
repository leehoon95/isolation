using UnityEngine;

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
 * 충돌한 반대편 projectile에게 효과 타입
 */
public enum CollisionEffect : int
{
	None = 0,
	Damage = 1,
	DamageAndDebuff,
	Debuff,
	Item
}

/*
 * 충돌 후 반대편 collider interactable 객체에게 전달되는 이벤트
 */
public class CollisionEvent
{
	public Vector2 Position; // 충돌한 object의 위치
	public Vector2 Direction; // 충돌한 object의 이동 방향
	public CollisionEffect Effect; // 효과
	public string EffectDetail; // 자세한 효과
}

/*
 * collider를 통한 상호작용을 위한 interface
 * 충돌시 바로 이벤트를 실행하는 것이 아니라 object 내부 이벤트 큐에 등록하여
 * object 스스로 처리할 수 있도록 할 것
 */
public interface ICollisionInteractable
{
	void AddCollisionEvent(CollisionEvent ce);
	CollisionEffect GetEffect();
}
