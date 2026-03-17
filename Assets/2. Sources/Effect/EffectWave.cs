using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class EffectWave : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	float _processMultiplier = 2.5f;
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

	MaterialPropertyBlock _materialPropertyBlock;

	void Awake()
	{
		_materialPropertyBlock = new();
	}

	void OnEnable()
	{
		StartCoroutine(ProcessWave());
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator ProcessWave()
	{
		float time = 0f;

		while (time < 1f)
		{
			_spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
			_materialPropertyBlock.SetFloat("_WaveThickness", Mathf.Lerp(_startThickness, _endThickness, time));
			_materialPropertyBlock.SetFloat("_WaveStrength", Mathf.Lerp(_startStrength, _endStrength, time));
			_materialPropertyBlock.SetFloat("_WaveDistanceFromCenter", Mathf.Lerp(_startWaveDistanceFromCenter, _endDistanceFromCenter, time));
			_spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
			time += Time.deltaTime * _processMultiplier;
			yield return null;
		}

		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{

	}

#if UNITY_EDITOR
	public void StartWaveCoroutin()
	{
		StopAllCoroutines();
		StartCoroutine(ProcessWave());
	}
#endif

}
