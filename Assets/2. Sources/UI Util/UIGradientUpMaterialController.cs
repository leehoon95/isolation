using UnityEngine;

[ExecuteAlways]
public class UIGradientUpMaterialController : UIMaterialControllerBase
{
	[SerializeField]
	Color _color = Color.red;
	[SerializeField]
	[Range(0f, 1f)]
	float _offset = 0.5f;

	protected override void EditMaterialPropertiesValue()
	{
		material.SetColor("_Color", _color);
		material.SetFloat("_Offset", _offset);
	}
}
