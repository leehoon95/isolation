using Unity.VisualScripting;
using UnityEngine;

/*
 * deprecate
 * MaterialPropertyBlock 또는 ParticleSystem 인터페이스를 사용할 것
 */
public class ParticleMaterialController : MonoBehaviour
{
	[SerializeField]
	ParticleSystem _particleSystem;

	//ParticleSystemRenderer _particleSystemRenderer;
	Material _material;

	void Start()
	{
		//_particleSystemRenderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
		//var emission = _particleSystem.emission;
		//var burst = emission.GetBurst(0);
		//burst.count = new ParticleSystem.MinMaxCurve(_minParticle, _maxParticle);
		//emission.SetBurst(0, burst);
	}

	void OnEnable()
	{
		if (_material == null)
		{
			var pcr = _particleSystem.GetComponent<ParticleSystemRenderer>();
			_material = pcr.material;
			_material.hideFlags = HideFlags.HideAndDontSave;
			pcr.material = _material;
		}
	}

	void OnDestroy()
	{
		if (_material != null)
		{
#if UNITY_EDITOR
			DestroyImmediate(_material);
#else
			Destroy(_material);
#endif
		}
	}
	
	public void SetLifeTime(float minTime, float maxTime)
	{
		var main = _particleSystem.main;
		main.startLifetime = new ParticleSystem.MinMaxCurve() { 
			mode = ParticleSystemCurveMode.TwoConstants,
			constantMin = minTime,
			constantMax = maxTime
		};
		main.duration = maxTime + 1f;
	}

	public void Stop()
	{
		_particleSystem.Stop();
	}

	public void Play()
	{
		_particleSystem.Play();
	}

	public void SetColor(Color color)
	{
		if (_material)
		{
			_material.SetColor("_Color", color);
		}
	}
}
