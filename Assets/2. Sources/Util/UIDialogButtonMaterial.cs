using UnityEngine;

public class UIDialogButtonMaterial : UIMaterialControllerBase
{
	[SerializeField]
	Color _color;

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		base.OnValidate();

		SetMaterialDirty();
	}
#endif

	protected override void EditMaterialPropertiesValue()
	{ 
		base.EditMaterialPropertiesValue();

		material.SetColor("_Color", _color);
	}
}