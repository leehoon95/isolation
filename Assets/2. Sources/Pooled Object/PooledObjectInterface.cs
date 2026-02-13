using UnityEngine;

/*
 * Pooled dynamic spawner에서 projectile 초기화를 위한 인터페이스
 */
public interface IProjectileSetting
{
	void SetProjectileParameter(in ProjectileRpcParameter param);
}


/*
 * Pooled dynamic spawner에서 effect 초기화를 위한 인터페이스
 */
public interface IEffectSetting
{
	void SetEffectParameter(in EffectRpcParameter param);
}