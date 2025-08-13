using UnityEngine;

public class UINotificationBackground : UIMaterialControllerBase
{
	[SerializeField]
	[Range(0f, 1f)]
	float _noiseStrengthMin = 0f;
	[SerializeField]
	[Range(0f, 1f)]
	float _noiseStrengthMax = 1f;
	[SerializeField]
	Vector2 _noiseDirection = Vector2.zero;
	[SerializeField]
	float _pixel = 20f;
	[SerializeField]
	bool _warning;

	protected override void EditMaterialPropertiesValue()
	{
		base.EditMaterialPropertiesValue();

		material.SetFloat("_NoiseStrengthMin", _noiseStrengthMin);
		material.SetFloat("_NoiseStrengthMax", _noiseStrengthMax);
		material.SetVector("_NoiseDirection", _noiseDirection);
		material.SetFloat("_Pixel", _pixel);
		material.SetInt("_Warning", _warning ? 1 : 0);
	}
}
