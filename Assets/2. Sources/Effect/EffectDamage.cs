using System.Collections;
using TMPro;
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

	WaitForSeconds _delay1 = new(1.5f);
	WaitForSeconds _delay2 = new(0.1f);

	void OnEnable()
	{
		StartCoroutine(ProcessShowDamage());
	}

	IEnumerator ProcessShowDamage()
	{
		yield return null;
		_sortingGroup.sortingOrder = 99;
		_animator.Play("EffectDamageOn", -1, 0f);
		yield return _delay1;
		_animator.Play("EffectDamageOff", -1, 0f);
		yield return _delay2;
		ReleaseObject();
	}


	public void SetEffectParameter(in EffectRpcParameter param)
	{
		_text.color = param.EffectColor;
		_text.text = ((int)param.Data1).ToString();
	}
}
