using UnityEngine;


public class PlayerBodyIndicator : MonoBehaviour
{
	[SerializeField]
	SpriteRenderer _bodyRenderer;
	[Range(0, 100)]
	[SerializeField]
	int _health;
	[SerializeField]
	Color _personalColor = Color.white;
	[SerializeField]
	Color _fatalSignColor = Color.black;

	MaterialPropertyBlock _materialPropertyBlock;

	public Color PersonalColor 
	{
		set 
		{ 
			_personalColor = value;
			_fatalSignColor = Color.white - value;
			UpdateColorMaterialProperty();
		}
	}

	public int Health
	{
		get => _health;
		set
		{
			_health = Mathf.Clamp(value, 0, 100);
			UpdateColorMaterialProperty();
		}
	}

#if UNITY_EDITOR
	void OnValidate()
	{
		UpdateColorMaterialProperty();
	}
#endif

	void FixedUpdate()
	{
		UpdateRotationMaterialProperty();
	}

	void UpdateColorMaterialProperty()
	{
		if (_materialPropertyBlock == null)
		{
			_materialPropertyBlock = new();
		}

		var rate = _health / 100f;
		_bodyRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", Color.Lerp(_fatalSignColor, _personalColor, Mathf.Clamp01(rate / 0.25f)));
		_materialPropertyBlock.SetFloat("_Value", rate);
		//_materialPropertyBlock.SetFloat("_Rotation", -transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
		_bodyRenderer.SetPropertyBlock(_materialPropertyBlock);
	}

	void UpdateRotationMaterialProperty()
	{
		if (_materialPropertyBlock == null)
		{
			_materialPropertyBlock = new();
		}

		_bodyRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetFloat("_Rotation", -transform.rotation.eulerAngles.z * Mathf.Deg2Rad);
		_bodyRenderer.SetPropertyBlock(_materialPropertyBlock);
	}
}
