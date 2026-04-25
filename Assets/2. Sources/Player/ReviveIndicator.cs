using UnityEngine;

public class ReviveIndicator : MonoBehaviour
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;

	MaterialPropertyBlock _materialPropertyBlock;
	float _progress;

	public float Progress
	{
		get => _progress;
		set => UpdateIndicator(value);
	}

	void OnEnable()
	{
		if (_materialPropertyBlock == null)
		{
			_materialPropertyBlock = new();
		}
		UpdateIndicator(0f);
	}

	void UpdateIndicator(float progress)
	{
		_progress = Mathf.Clamp01(progress);

		_spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetFloat("_FillAmount", _progress);
		_spriteRenderer.SetPropertyBlock(_materialPropertyBlock);
	}
}
