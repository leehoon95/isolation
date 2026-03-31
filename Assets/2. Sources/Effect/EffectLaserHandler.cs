using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class EffectLaserHandler : MonoBehaviour
{
	[SerializeField]
	LineRenderer _lineRenderer;
	[SerializeField]
	GameObject _flameObject;
	[SerializeField]
	SpriteRenderer _outFlame;
	[SerializeField]
	SpriteRenderer _innerFlame;
	//[SerializeField]
	//EffectCross _crossFlame;
	[SerializeField]
	Animator _animator;
	[SerializeField]
	Color _effectColor;

	MaterialPropertyBlock _outMaterialPropertyBlock;
	MaterialPropertyBlock _innerMaterialPropertyBlock;
	MaterialPropertyBlock _lineMaterialPropertyBlock;
	Coroutine _FiringCo;

	public Vector2 StartPosition
	{
		set
		{
			_lineRenderer.SetPosition(0, transform.InverseTransformPoint(value));
		}
	}

	public Vector2 EndPosition
	{
		set
		{
			_lineRenderer.SetPosition(1, transform.InverseTransformPoint(value));
			_outFlame.transform.SetPositionAndRotation(value, Quaternion.identity);
			_innerFlame.transform.SetPositionAndRotation(value, Quaternion.identity);
		}
	}

	public bool IsShow { get => _lineRenderer.gameObject.activeSelf; }

	public Color EffectColor
	{
		set
		{
			_effectColor = value;
		}
	}

	public float FiringDuration { get; set; }

	void Start()
	{
		_outMaterialPropertyBlock = new();
		_innerMaterialPropertyBlock = new();
		_lineMaterialPropertyBlock = new();

		UpdateColor();
	}

	public void UpdateColor()
	{
		_outFlame.GetPropertyBlock(_outMaterialPropertyBlock);
		_outMaterialPropertyBlock.SetColor("_Color", _effectColor);
		_outFlame.SetPropertyBlock(_outMaterialPropertyBlock);
		_innerFlame.GetPropertyBlock(_innerMaterialPropertyBlock);
		_innerMaterialPropertyBlock.SetColor("_Color", Color.white);
		_innerFlame.SetPropertyBlock(_innerMaterialPropertyBlock);
		_lineRenderer.GetPropertyBlock(_lineMaterialPropertyBlock);
		_lineMaterialPropertyBlock.SetColor("_Color", _effectColor);
		_lineRenderer.SetPropertyBlock(_lineMaterialPropertyBlock);
	}

	public void Show()
	{
		_lineRenderer.gameObject.SetActive(true);
		_flameObject.SetActive(true);
		_animator.SetTrigger("On");
	}

	public void Stop()
	{
		_lineRenderer.gameObject.SetActive(false);
		_flameObject.SetActive(false);
	}
}
