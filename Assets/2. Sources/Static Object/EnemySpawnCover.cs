using System;
using UnityEngine;

// deprecated
public class EnemySpawnCover : MonoBehaviour
{
	[SerializeField]
	Transform _gradient;
	[SerializeField]
	Transform _cover;
	[Space]
	[SerializeField]
	float _coverHeight;
	[SerializeField]
	float _coverWidth;

#if UNITY_EDITOR
	void OnValidate()
	{
		GLogger.Log("onvalidate");
		SetCoverLength(_coverWidth, _coverHeight);
	}
#endif

	void SetCoverLength(float width, float height)
	{
		var s = _gradient.localScale;
		s.x = width;
		_gradient.localScale = s;
		_cover.localScale = new Vector3(width, height, 1f);
		_cover.localPosition = new Vector3(0f, -(height - 1f) / 2f, 0f);
	}
}
