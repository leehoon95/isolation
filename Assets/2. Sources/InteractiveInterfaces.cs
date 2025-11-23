using System;
using UnityEngine;

[Serializable]
public struct ProjectilePhysicsData
{

}

/*
 * collider를 통한 상호작용을 위한 interface
 * 
 */

public interface IColliderInteractable
{
    Vector2 StartFrom { get; set; }
    void AddForce(Vector2 force);
}
