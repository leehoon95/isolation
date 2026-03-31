using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EffectPop : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	ParticleSystem _particleSystem;

	Color _effectColor;

	void OnEnable()
	{
		_particleSystem.Stop();
		var main = _particleSystem.main;
		var lifeTime = main.startLifetime;
		StartCoroutine(PlayParticleSystem(lifeTime.constantMax));
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator PlayParticleSystem(float lifeTime)
	{
		yield return null;
		var main = _particleSystem.main;
		main.startColor = _effectColor;
		_particleSystem.Play();

		yield return new WaitForSeconds(lifeTime);

		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		_effectColor = param.EffectColor;
	}
}
