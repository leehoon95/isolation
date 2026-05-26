using UnityEngine;

public class MaintainPPU : MonoBehaviour
{
	[SerializeField]
	Transform _transformTarget;
	[SerializeField]
	SpriteRenderer _spriteRenderer;

	void OnEnable()
	{
		Maintain();
	}

	//#if UNITY_EDITOR
	//	void OnValidate()
	//	{
	//		Maintain();
	//	}
	//#endif

	public void Maintain()
	{
		var scale = _transformTarget.localScale;
		if (_spriteRenderer.drawMode == SpriteDrawMode.Simple)
		{
			// ...
		}
		else if (_spriteRenderer.drawMode == SpriteDrawMode.Sliced)
		{
			_spriteRenderer.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
			_spriteRenderer.size = scale;
		}
		else if (_spriteRenderer.drawMode == SpriteDrawMode.Tiled)
		{
			// ...
		}
	}
}
