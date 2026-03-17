using System.Collections;
using UnityEngine;

public class EffectHitNotmal : PooledEffectBase, IEffectSetting
{
    [SerializeField]
	Animator _animator;
	[SerializeField]
	SpriteRenderer _spriteRenderer;
	[SerializeField]
	ParticleMaterialController _pmc;

	void OnEnable()
	{
		//_animator.Rebind();

		//var info = _animator.GetCurrentAnimatorStateInfo(0);
		//GLogger.Log($"len {info.length}");
		var length = 0.333f;
		StartCoroutine(PlayAnimation(length));
		_pmc.Stop();
		_pmc.SetColor(Color.white);
		var halfRange = length * 0.5f;
		_pmc.SetLifeTime(length - halfRange, length);
		_pmc.Play();
	}

	void OnDisable()
	{
		StopAllCoroutines();
	}

	IEnumerator PlayAnimation(float length)
	{
		yield return new WaitForSeconds(length);
		ReleaseObject();
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
		//throw new System.NotImplementedException();
	}
}
