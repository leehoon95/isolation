using Mono.Cecil.Cil;
using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectDamage : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	Animator _animator;
	[SerializeField]
	TMP_Text _text;
	[SerializeField]
	SortingGroup _sortingGroup;
	[SerializeField]
	float _z;

	WaitForSeconds _delay1 = new(1.5f);
	WaitForSeconds _delay2 = new(0.1f);

	void OnEnable()
	{
		StartCoroutine(ProcessShowDamage());
	}

	IEnumerator ProcessShowDamage()
	{
		yield return null;
		float t = 0f;
		_animator.Play("EffectDamageOn", -1, 0f);

		while (t < 1f)
		{
			var pos = transform.position;
			pos.z = _z;
			transform.position = pos;
			t += Time.deltaTime;
			yield return null;
		}

		_animator.Play("EffectDamageOff", -1, 0f);
		t = 0f;
		while (t < 0.1f)
		{
			var pos = transform.position;
			pos.z = _z;
			transform.position = pos;
			t += Time.deltaTime;
			yield return null;
		}

		ReleaseObject();
	}


	public void SetEffectParameter(in EffectRpcParameter param)
	{
		_text.faceColor = param.EffectColor;

		int offset = 0;
		int damage = 0;
		var data = param.Data;
		var error = data.Parse(ref offset, ref damage);
		if (error == ParseError.None)
		{
			_text.text = damage.ToString();
		}
		
	}
}
