using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class EffectGuidanceIndicator : PooledEffectBase, IEffectSetting
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	TMP_Text _text;
	[SerializeField]
	SpriteRenderer _rightGuide;
	[SerializeField]
	SpriteRenderer _topGuide;
	[SerializeField]
	SpriteRenderer _leftGuide;
	[SerializeField]
	SpriteRenderer _bottomGuide;
	[SerializeField]
	SpriteRenderer _centerSprite;
	[SerializeField]
	SpriteRenderer circleSprite;
	[Header("Spec")]
	[SerializeField]
	string _projectileName;
	[SerializeField]
	float _aimingTime;
	[SerializeField]
	float _crosshairSpeed;
	[SerializeField]
	int _bombCount;
	[SerializeField]
	int _damage;

	Coroutine _co;
	IGameProjcessorInterface _GP;
	Color _effectColor;

	void Start()
	{
		_GP = FindAnyObjectByType<GameProcessor>();
		if (_GP == null)
		{
			GLogger.Log("Not found GameProcessor");
		}
	}

	void OnEnable()
	{
		if (_co != null)
		{
			StopCoroutine(_co);
		}

		_co = StartCoroutine(Aim());
	}

	void OnDisable()
	{
		StopAllCoroutines();
		_co = null;
	}

	IEnumerator Aim()
	{
		float t = 0f;
		var currVelocity = Vector2.zero;
		yield return null;

		while (t <= _aimingTime)
		{
			if (!_GP.IsPlayerSpawned)
			{
				ReleaseObject();
				_rigidbody.linearVelocity = Vector2.zero;
				yield break;
			}
			SetIndicatorMotion(t);

			var mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
			var vecToMouse = (Vector2)(mousePosition - transform.position);
			var distance = vecToMouse.magnitude;
			var norVecToMouse = vecToMouse.normalized;
			var maxVelocity = vecToMouse.normalized * _crosshairSpeed;
			if (maxVelocity.sqrMagnitude < vecToMouse.sqrMagnitude)
			{
				_rigidbody.linearVelocity = maxVelocity;
			}
			else if (distance < 1.75f
				&& distance > 0.001f)
			{
				Vector2.SmoothDamp(transform.position, mousePosition, ref currVelocity, 0.3333f);
				_rigidbody.linearVelocity = currVelocity;
			}

			t += Time.deltaTime;
			yield return null;
		}

		_rigidbody.linearVelocity = Vector2.zero;
		SetIndicatorMotion(_aimingTime);
		yield return null;

		t = 0f;
		int count = _bombCount;
		var prp = new ProjectileRpcParameter()
		{
			CollisionEvent = new CollisionEventStruct()
			{
				SenderId = OwnerClientId,
				Effect = CollisionEffect.Knockback,
				EffectDuration = 0.15f,
				EffectIntensity = 5f,
				Damage = _damage
			},
			EffectColor = _effectColor,
			LifeTime = 1f
		};

		var erp = new EffectRpcParameter()
		{
			EffectColor = _effectColor
		};

		while (count > 0)
		{
			if (t > 0.4f)
			{
				var position = (Vector2)transform.position + Random.insideUnitCircle * 1.75f;
				IPDS.CreateProjectile(
					_projectileName,
					position,
					Quaternion.identity,
					prp);

				IPDS.CreateEffect(
					"EffectWave",
					position,
					Quaternion.identity,
					erp);

				IPDS.CreateEffect(
					"EffectNoiseBig",
					position,
					Quaternion.identity,
					erp);

				t = 0f;
				count--;
			}

			t += Time.deltaTime;
			yield return null;
		}

		_co = null;
		ReleaseObject();
	}

	void SetIndicatorMotion(float t)
	{
		int progress = Mathf.CeilToInt(Mathf.Lerp(0f, 100f, t / _aimingTime));
		_text.text = progress.ToString();

		var alpha = 0.25f + 0.75f * Mathf.PingPong(t, 1f);
		var color = _centerSprite.color;
		color.a = alpha;
		_centerSprite.color = color;

		var radius = Mathf.Lerp(1.6f, 0.125f, t / _aimingTime);
		_rightGuide.transform.localPosition = new Vector2(radius, 0f);
		_rightGuide.color = color;
		_topGuide.transform.localPosition = new Vector2(0f, radius);
		_topGuide.color = color;
		_leftGuide.transform.localPosition = new Vector2(-radius, 0f);
		_leftGuide.color = color;
		_bottomGuide.transform.localPosition = new Vector2(0f, -radius);
		_bottomGuide.color = color;
	}

	public void SetEffectParameter(in EffectRpcParameter param)
	{
	
		_effectColor = param.EffectColor;
		GLogger.Log($"indicatgor color {_effectColor}");
	}
}
