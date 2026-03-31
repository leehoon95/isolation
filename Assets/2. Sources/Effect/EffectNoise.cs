using System.Collections;
using UnityEngine;

public class EffectNoise : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	ParticleSystem _particleSystem;
	[SerializeField]
	float _particleLifeTime = 2f;
	[SerializeField]
	float _noiseLifeTime = 1f;
	[SerializeField]
	float _startNoiseStrength = 0.5f;
	[SerializeField]
	float _endNoiseStrength = 0f;
	[SerializeField]
	Color _areaColor = Color.red;
	[SerializeField]
	float _startAreaOpacity = 1f;
	[SerializeField]
	float _endAreaOpacity = 0f;
	[SerializeField]
	float _startAlpha = 1f;
	[SerializeField]
	float _endAlpha = 0f;
	[SerializeField]
	bool _reduceSizeEffect;

	MaterialPropertyBlock _materialPropertyBlock;

	void Awake()
	{
		_materialPropertyBlock = new();
	}

	void OnEnable()
	{
		_particleSystem.Stop();
		transform.localScale = new Vector3(3f, 3f, 1f);
	
		StartCoroutine(ProcessNoise());

		if (_reduceSizeEffect)
		{
			StartCoroutine(ReduceSizeAndRelease());
		}
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator ProcessNoise()
	{
		yield return null;
		
		float time = 0f;
		float endTime = Mathf.Max(_particleLifeTime, _noiseLifeTime);
		var main = _particleSystem.main;
		main.startLifetime = _particleLifeTime;
		main.startColor = _areaColor;
		_particleSystem.Play();

		while (time < endTime)
		{
			_spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
			_materialPropertyBlock.SetFloat("_NoiseStrength", Mathf.Lerp(_startNoiseStrength, _endNoiseStrength, time / _noiseLifeTime));
			_materialPropertyBlock.SetFloat("_Alpha", Mathf.Lerp(_startAlpha, _endAlpha, time / _noiseLifeTime));
			_spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
			time += Time.deltaTime;
			yield return null;
		}

		ReleaseObject();
	}

	IEnumerator ReduceSizeAndRelease()
	{
		float time = 0f;
		float endTime = Mathf.Max(_particleLifeTime, _noiseLifeTime);
		var scale = transform.localScale;
		yield return null;

		while (time < endTime)
		{
			var s = Mathf.Lerp(scale.x, 0f, time / endTime);
			transform.localScale = new Vector3(s, s, 0f);

			time += Time.deltaTime;
			yield return null;
		}
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		_areaColor = param.EffectColor;
	}
}
