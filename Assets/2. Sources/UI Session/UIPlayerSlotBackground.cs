using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(Image), typeof(RectTransform))]
public class UIPlayerSlotBackground : UIBehaviour, IMaterialModifier
{
	[SerializeField]
	Color _patternColor = new();
	[SerializeField]
	[Range(0f, 1f)]
	float _offset = 0.5f;

	Material _material;
	Graphic _graphic;


	public Color PatternColor
	{
		get => _patternColor;
		set
		{
			_patternColor = value;
			SetShaderGraphProperty();
		}
	}

	public float Offset
	{
		get => _offset;
		set
		{
			_offset = value;
			SetShaderGraphProperty();
		}
	}

	protected override void OnEnable()
	{
		if (TryGetComponent<Graphic>(out _graphic))
		{
			_graphic.SetMaterialDirty();
		}
	}

	protected override void OnDisable()
	{
		if (_material != null)
		{
#if UNITY_EDITOR
			DestroyImmediate(_material);
#else
			Destroy(_material);
#endif
		}

		_material = null;
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		SetShaderGraphProperty();
	}
#endif

	protected override void OnRectTransformDimensionsChange()
	{
		SetShaderGraphProperty();
	}

	void SetShaderGraphProperty()
	{
		if (_material == null)
		{
			return;
		}

		var rt = (RectTransform)transform;

		//_material.SetVector(
		//	"_ObjectScale",
		//	new Vector2(rt.rect.width / _objectScaleUnit, rt.rect.height / _objectScaleUnit));
		_material.SetFloat("_Offset", _offset);
		_material.SetColor("_Color", _patternColor);

		_graphic.SetMaterialDirty();
	}

	public Material GetModifiedMaterial(Material baseMaterial)
	{
		if (_material == null)
		{
			_material = new Material(baseMaterial); // 이 오브젝트만의 material instance를 생성한다.
			_material.hideFlags = HideFlags.HideAndDontSave;
			_material.SetFloat("_Offset", _offset);
			_material.SetColor("_Color", _patternColor);
		}

		return _material;
	}
}
