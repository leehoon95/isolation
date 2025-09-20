using System.Collections;
using UnityEngine;

public class UIGlitchEffect : UIMaterialControllerBase
{
    [SerializeField] float _glitchAlpha = 1f;
    [SerializeField] float _glitchMoveHorizontal = 0f;
    [SerializeField] float _glitchMoveVertical = 0f;
	Coroutine _coroutin;
	

	protected override void OnEnable()
	{
		base.OnEnable();

		_coroutin = StartCoroutine(BlinkEffect());
	}

	protected override void OnDisable()
	{
		base.OnDisable();

		if (_coroutin != null)
		{
			StopCoroutine(_coroutin);
		}
	}

	IEnumerator BlinkEffect()
	{
		yield return null;

		while (true)
		{
			_glitchMoveVertical += 0.01f;
			SetMaterialDirty();
			yield return new WaitForSeconds(0.4f);
		}
	}
	protected override void EditMaterialPropertiesValue()
	{
		base.EditMaterialPropertiesValue();

		material.SetFloat("_GlitchMoveHorizontal", _glitchMoveHorizontal);
		material.SetFloat("_GlitchMoveVertical", _glitchMoveVertical);
		material.SetFloat("_GlitchAlpha", _glitchAlpha);
	}
}
