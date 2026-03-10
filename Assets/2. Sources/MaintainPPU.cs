using System.Collections.Generic;
using UnityEngine;

public class MaintainPPU : MonoBehaviour
{
	[SerializeField]
	List<SpriteRenderer> _spriteRenderer;

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
		foreach (var sr in _spriteRenderer)
		{
			var scale = transform.localScale;
			if (sr.drawMode == SpriteDrawMode.Simple)
			{ 
				// ...
			}
			else if (sr.drawMode == SpriteDrawMode.Sliced)
			{
				sr.transform.localScale = new Vector3(1f / scale.x, 1f / scale.y, 1f / scale.z);
				sr.size = scale;
			}
			else if (sr.drawMode == SpriteDrawMode.Tiled)
			{

			}
				
		}
	}
	
}
