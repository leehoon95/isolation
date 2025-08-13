using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UIMaterialControllerBase : UIBehaviour, IMaterialModifier
{
	[NonSerialized]
	Graphic _graphic;

	[NonSerialized]
	protected Material material;

	protected override void OnEnable()
	{
		base.OnEnable();

		SetMaterialDirty();
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		if (material != null)
		{
#if UNITY_EDITOR
			DestroyImmediate(material);
#else
			Destroy(material);
#endif
		}

		material = null;
		SetMaterialDirty();
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();
		SetMaterialDirty();
	}
#endif

	// animation을 통해 property가 변경되면 호출됨
	protected override void OnDidApplyAnimationProperties()
	{
		base.OnDidApplyAnimationProperties();

		if (!IsActive())
		{
			return;
		}

		SetMaterialDirty();
	}


	// Graphic material에 Dirty flag가 설정되면 호출됨
	Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
	{
		print("GetModifiedMaterial()");
		if (material == null)
		{
			material = new Material(baseMaterial);
			material.hideFlags = HideFlags.HideAndDontSave;
			/*
			 * Hide: Hierachy 뷰나 Project뷰에 이 오브젝트를 보이지 않게 숨김
			 * 런타임에는 정상적으로 존재
			 * 디버깅용, 임시로 생성된 리소스를 직접 건드릴 필요 없게 숨길 때 사용
			 * 
			 * DontSave: Scene 저장 시 같이 저장되지 않게 한다
			 * play 도중 동적으로 만든 Material이
			 * Scene파일에 기록되지 않고
			 * Editor를 껐다 켜도 남아 있지 않음
			 * 임시 리소소를 만들 때 메모리와 저장 공간 낭비를 방지 한다.
			 * 
			 * 즉, 런타임에만 필요한 임시 Material을 생성
			 */
		}

		EditMaterialPropertiesValue();

		return material;
	}

	protected virtual void EditMaterialPropertiesValue()
	{}

	protected void SetMaterialDirty()
	{
		if (_graphic == null)
		{
			_graphic = GetComponent<Graphic>();
		}

		_graphic.SetMaterialDirty();
	}
}
