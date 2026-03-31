using UnityEngine;

public class EffectCross : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _outFlame;
	[SerializeField]
	SpriteRenderer _innerFlame;
	[SerializeField]
	Color _flameColor;
	[SerializeField]
	Animator _animator;

	MaterialPropertyBlock _outMaterialPropertyBlock;
	MaterialPropertyBlock _innerMaterialPropertyBlock;

	void OnEnable()
	{
		_animator.SetTrigger("On");
		UpdateColor(_flameColor);
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	public Vector2 Position
	{
		set
		{
			_outFlame.transform.SetPositionAndRotation(value, Quaternion.identity);
			_innerFlame.transform.SetPositionAndRotation(value, Quaternion.identity);
		}
	}

	public Color Color
	{
		set
		{
			_flameColor = value;
			UpdateColor(value);
		}
	}

	void UpdateColor(Color color)
	{
		if (_outMaterialPropertyBlock == null)
		{
			_outMaterialPropertyBlock = new();
		}

		if (_innerMaterialPropertyBlock == null)
		{
			_innerMaterialPropertyBlock = new();
		}
		_outFlame.GetPropertyBlock(_outMaterialPropertyBlock);
		_outMaterialPropertyBlock.SetColor("_Color", color);
		_outFlame.SetPropertyBlock(_outMaterialPropertyBlock);
		//_innerFlame.GetPropertyBlock(_innerMaterialPropertyBlock);
		//_innerMaterialPropertyBlock.SetColor("_Color", Color.white);
		//_innerFlame.SetPropertyBlock(_innerMaterialPropertyBlock);
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		//_areaColor = param.EffectColor;
	}
}
