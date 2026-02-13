using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/*
 * ui object 전용 shader graph property(aspect ratio) 세팅용 스크립트
 * ui image, text는 MaterialPropertyBlock을 지원하지 않는다
 * 따라서, IMaterialModifier 인터페이스를 사용한다
 */
[ExecuteAlways]
[RequireComponent(typeof(Image))]
public class UIMaterialScaleNotifier : UIBehaviour, IMaterialModifier
{
    [SerializeField]
    float _aspectRatio;
	[SerializeField]
	Material _material;

	Graphic _graphic;

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

	protected override void OnRectTransformDimensionsChange()
	{
		if (_material == null)
		{
			return;
		}

		var rt = (RectTransform)transform;

		float aspectRatio = rt.rect.width / rt.rect.height;
		Debug.Log($"localScale {rt.rect.width} {rt.rect.height}");

		_material.SetFloat("_AspectRatio", aspectRatio);
		_material.SetVector("_ObjectScale", new Vector2(rt.rect.width / 64f, rt.rect.height / 64f));

		_graphic.SetMaterialDirty();
	}

	public Material GetModifiedMaterial(Material baseMaterial)
	{
		if (_material == null)
		{
			_material = new Material(baseMaterial); // 이 오브젝트만의 material instance를 생성한다.
			_material.hideFlags = HideFlags.HideAndDontSave;
		}

		return _material;
	}
}
