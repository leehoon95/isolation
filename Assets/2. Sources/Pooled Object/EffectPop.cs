using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EffectPop : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	ParticleSystem _particleSystem;

	Color _effectColor;

	Color EffectColor
	{
		get => _effectColor;
		set
		{
			_effectColor = value;
			var main = _particleSystem.main;
			main.startColor = _effectColor;
		}
	}

	void OnEnable()
	{
		_particleSystem.Play();
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
		yield return new WaitForSeconds(lifeTime);
		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		EffectColor = param.EffectColor;
	}
}
