using UnityEngine;

public class UIDialogButtonMaterial : UIMaterialControllerBase
{
	[SerializeField]
	Color _color;

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		SetMaterialDirty();
	}
#endif

	protected override void EditMaterialPropertiesValue()
	{
		material.SetColor("_Color", _color);
	}
}