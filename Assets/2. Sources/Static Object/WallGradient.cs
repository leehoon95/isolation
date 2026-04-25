using UnityEngine;



public class WallGradient : MonoBehaviour
{
	[SerializeField]
	SpriteRenderer _spriteRenderer;

	MaterialPropertyBlock _materialPropertyBlock;

	void Awake()
	{
		_materialPropertyBlock = new();
	}

	void Start()
	{
		_spriteRenderer.GetPropertyBlock(_materialPropertyBlock);
		_materialPropertyBlock.SetColor("_Color", Color.black);
		_spriteRenderer.SetPropertyBlock(_materialPropertyBlock);

	}
}
