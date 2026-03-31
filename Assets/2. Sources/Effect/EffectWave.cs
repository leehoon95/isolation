using System.Collections;
using UnityEngine;

public class EffectWave : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	float _waveSpeedMultiplier = 1f;
	[SerializeField]
	float _duration;
	[SerializeField]
	float _startThickness = 0f;
	[SerializeField]
	float _endThickness = 0.1f;
	[SerializeField]
	float _startStrength = 0.2f;
	[SerializeField]
	float _endStrength = 0f;
	[SerializeField]
	float _startWaveDistanceFromCenter = 0.01f;
	[SerializeField]
	float _endDistanceFromCenter = 0.4f;
	[SerializeField]
	float _particleLifeTime;

	Color _effectColor;
	MaterialPropertyBlock _materialPropertyBlock;

	void Awake()
	{
		_materialPropertyBlock = new();
	}

	void OnEnable()
	{
		StartCoroutine(ProcessWave(_duration));
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator ProcessWave(float duration)
	{
		yield return null;

		float time = 0f;

		while (time < duration)
		{
			_spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
			_materialPropertyBlock.SetFloat("_WaveThickness", Mathf.Lerp(_startThickness, _endThickness, time));
			_materialPropertyBlock.SetFloat("_WaveStrength", Mathf.Lerp(_startStrength, _endStrength, time));
			_materialPropertyBlock.SetFloat("_WaveDistanceFromCenter", Mathf.Lerp(_startWaveDistanceFromCenter, _endDistanceFromCenter, time * time * _waveSpeedMultiplier));
			_spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
			time += Time.deltaTime;
			yield return null;
		}

		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		_effectColor = param.EffectColor;
	}
}
