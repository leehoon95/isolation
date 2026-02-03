using UnityEngine;



[CreateAssetMenu(fileName = "ProjectileStatusSO", menuName = "Scriptable Objects/ProjectileStatusSO")]
public class ProjectileStatusSO : ScriptableObject
{
    public float Velocity;
    public float ImpulseIntensity;
    public LayerMask CollisionIncludeLayers;
    public LayerMask CollisionExcludeLayers; // include layer보다 우선 순위 높음
}
