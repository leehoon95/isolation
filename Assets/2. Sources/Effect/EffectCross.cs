using UnityEngine;

public class EffectCross : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;

	MaterialPropertyBlock _materialPropertyBlock;

	void Awake()
	{
		_materialPropertyBlock = new();
	}

	void OnEnable()
	{

		//StartCoroutine(ProcessNoise());
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}



	public void SetEffectParameter(in EffectRpcParameter param)
	{
		//_areaColor = param.EffectColor;
	}
}
