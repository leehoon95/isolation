using System.Collections;
using UnityEngine;


public class EffectBurst : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	SpriteRenderer _body;
	[SerializeField]
	SpriteRenderer _bodyEdge;
	[SerializeField]
	SpriteRenderer _leftWeapon;
	[SerializeField]
	SpriteRenderer _rightWeapon;
	[SerializeField]
	SpriteRenderer _frontWeapon;
	[SerializeField]
	float _duration;
	[SerializeField]
	int _hueRound;
#if UNITY_EDITOR
	[SerializeField]
	Color _testColor;
#endif
	Coroutine _fadeoutCo;

	public Color EffectColor { get; set; }

	void OnEnable()
	{
		var pos = transform.position;
		transform.position = new Vector3(pos.x, pos.y, 1f);
		StartCoroutine(FadeoutAndRoundHue());
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator FadeoutAndRoundHue()
	{
		yield return null;
		float h, s, v;
		float t = 0f;
		float hueRoundTime = _duration / _hueRound;
		var pos = transform.position;

		Color.RGBToHSV(EffectColor, out h, out s, out v);

		v *= 0.5f;
		
		while (t < _duration)
		{
			float h2 = h + t / hueRoundTime;
			Color c = Color.HSVToRGB(h2 - Mathf.Floor(h2), s, v);
			var ratio = (1f - t / _duration);
			
			c *= ratio;
			transform.position = new Vector3(pos.x, pos.y, ratio);

			_body.color = c;
			_bodyEdge.color = c;
			_leftWeapon.color = c;
			_rightWeapon.color = c;
			_frontWeapon.color = c;

			t += Time.deltaTime;
			yield return null;
		}

		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		EffectColor = param.EffectColor;
	}
}
