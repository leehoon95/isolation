using UnityEngine;

/*
 * None : 운동하지 않음
 * Direct : 등속 직선 운동
 * Registed : 감속 직선 운동
 * Homing : 추적으로 인한 회전, 가감속 운동
 */
public enum ProjectileFlyingType
{
	None,
	Direct,
	Registed,
	Homing
}