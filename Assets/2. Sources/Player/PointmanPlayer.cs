using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
[RequireComponent(typeof(Rigidbody2D))]
public class PointmanPlayer : NetworkBehaviour, IPlayerHandler, INetworkObjectCollision
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	CircleCollider2D _collider;
	[SerializeField]
	CircleCollider2D _colliderTrigger;
	[SerializeField]
	WeaponContainer _weaponContainer;
	[SerializeField]
	SpriteRenderer _bodyInlineSprite;
	[SerializeField]
	PlayerBodyIndicator _bodyIndicator;
	[SerializeField]
	PlayerHand _hand;
	[SerializeField]
	PlayerCameraTarget _cameraTarget;
	[Header("Spec")]
	[SerializeField]
	float _speed;

	NetworkVariable<int> _health = new(
		6, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Owner);
	NetworkVariable<int> _shield = new(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);
	IWeaponInterface _leftWeapon;
	IWeaponInterface _rightWeapon;
	IPlayerSpawnObserver _spawnObserver;
	UILevelSO _uiso;
	PlayerSpawner _spawner;
	IItemHandler _grabbedItem;
	bool _grabbed;

	Vector2 _inputMovement;
	bool _inputLeftTrigger;
	bool _inputRightTrigger;

	string _nickname;
	Color _personalColor;
	float _angle;
	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEventCache = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Block,
		EffectDuration = 0f,
		Damage = 0,
	};
	long _lastHealthRecoveryTime;
	Dictionary<string, Coroutine> _buffApplied = new();

	public string Nickname
	{
		set => _nickname = value;
	}

	public Color PersonalColor 
	{
		get => _personalColor;
		set
		{
			_personalColor = value;
			_bodyIndicator.PersonalColor = value;
			_bodyInlineSprite.color = value;
		}
	}

	public InputSystem InputSystem { get; set; }
	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IPlayerSpawnObserver SpawnObserver { set => _spawnObserver = value; }
	public Transform CameraTarget => _cameraTarget.transform;
	public IPooledDynamicSpawner IPDS { get; set; }

	void Awake()
	{
		_collisionEventCache.SenderId = NetworkObjectId;
	}

	public override void OnNetworkSpawn()
	{
		OnPlayerSpawned();
	}


	public override void OnNetworkDespawn()
	{
		OnPlayerDespawned();
	}

	void Update()
	{
		if (!IsOwner)
		{
			return;
		}

		transform.rotation = Quaternion.Euler(0f, 0f, _angle);

		if (_uiso.IsShowingItemPicker())
		{
			_uiso.MoveItemPicket(_grabbedItem.GO.transform.position);
		}

		_uiso.UpdateIndicatorPosition(transform.position);
	}

	void FixedUpdate()
	{
		if (!IsOwner)
		{
			return;
		}

		while (_collisionEventList.Count > 0)
		{
			var ce = _collisionEventList[0];
			_collisionEventList.RemoveAt(0);

			if (_shield.Value > 0)
			{
				_shield.Value -= ce.Damage > 0 ? 1 : 0;
				GLogger.Log($"Health {_shield.Value}");
			}
			else
			{
				_health.Value -= ce.Damage > 0 ? 1 : 0;
			}

			if (ce.Effect > CollisionEffect.None
				&& ce.Effect < CollisionEffect.Block)
			{
				var closestPoint = _colliderTrigger.ClosestPoint(ce.Position);
				var erp = new EffectRpcParameter()
				{
					EffectColor = Color.red
				};
				erp.Data.Append(1);

				IPDS.CreateEffect(
					"EffectDamage",
					closestPoint,
					Quaternion.identity,
					erp);
			}
		}

		long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
		if (now - _lastHealthRecoveryTime > 10000)
		{
			_health.Value = Mathf.Min(_health.Value + 1, 6);

			_lastHealthRecoveryTime = now;
		}

		_health.Value = Mathf.Clamp(_health.Value, 0, 100);

		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var toMouseVector = mousePosition - (Vector2)transform.position;
		//float gunCorrectionAngle = 0f;

		/*
		 * 캐릭터 한 쪽에 정확히 조준해야 하는 로직이 있어야 하는 경우
		 */
		//if (_rightWeapon != null)
		//{
		//	var distance = (mousePosition - (Vector2)transform.position).magnitude;
		//	var gunWorldVector = _rightWeapon.MuzzleTransform.position - transform.position;
		//	/*
		//	 * 마우스가 캐릭터 중심과 gun muzzle 사이에 있으면 계산 불가
		//	 */
		//	if (distance > gunWorldVector.magnitude)
		//	{
		//		var gunX = Mathf.Abs(_rightWeaponPosition.transform.localPosition.x + _rightWeapon.MuzzleTransform.localPosition.x);
		//		var gunTargetY = Mathf.Sqrt(distance * distance - gunX * gunX);
		//		gunCorrectionAngle = Vector2.Angle(
		//			new Vector2(gunX, gunTargetY),
		//			new Vector2(0f, distance));
		//	}
		//	else
		//	{
		//		gunCorrectionAngle = 0f;
		//	}
		//}

		_angle = Mathf.Atan2(toMouseVector.y, toMouseVector.x) * Mathf.Rad2Deg;// + gunCorrectionAngle;

		if (_inputMovement.sqrMagnitude > float.Epsilon)
		{
			var newPosition = _rigidbody.position + _inputMovement.normalized * _speed * Time.fixedDeltaTime;
			_rigidbody.MovePosition(newPosition);
		}

		_weaponContainer.TargetPosition = mousePosition;
		_weaponContainer.Trigger(_inputLeftTrigger);
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		if (!IsOwner)
		{
			return;
		}

		var ci = collision.gameObject.GetComponentInParent<INetworkObjectCollision>();
		if (ci != null)
		{
			ci.SendCollisionEvent(_collisionEventCache);
			return;
		}

		var itemHandler = collision.gameObject.GetComponentInParent<IItemHandler>();
		if (itemHandler != null)
		{
			if (itemHandler.ItemType != ItemType.Buff)
			{
				return;
			}

			var effect = itemHandler.ItemEffect;
			if (effect == "burst")
			{
				if (_buffApplied.TryGetValue("burst", out var co))
				{
					StopCoroutine(co);
				}
				_buffApplied["burst"] = StartCoroutine(Burst(7f));
				itemHandler.DespawnItemRpc();
			}
			else if (effect == "shield")
			{
				_shield.Value = 3;
			}
			else if (effect == "bomb")
			{
				IPDS.CreateEffect(
					"EffectBombGuidanceIndicator",
					transform.position,
					Quaternion.identity,
					new EffectRpcParameter()
					{
						EffectColor = _personalColor
					});
			}
			else if (effect == "heal")
			{
				_health.Value += 1;
			}
			else
			{
				GLogger.Log($"Unknown item {effect}");
			}

			itemHandler.DespawnItemRpc();
		}
	}

	void OnPlayerSpawned()
	{
		_spawnObserver.NotifyPlayerSpawned(this);
		_weaponContainer.PersonalColor = PersonalColor;

		if (!IsOwner)
		{
			_hand.gameObject.SetActive(false);
			_collider.gameObject.SetActive(false);
			_colliderTrigger.gameObject.SetActive(false);
			return;
		}

		_collider.gameObject.SetActive(true);
		_colliderTrigger.gameObject.SetActive(true);
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;

		_health.Value = 6;
		_shield.Value = 0;
		_health.OnValueChanged += StatusChanged;
		_shield.OnValueChanged += StatusChanged;
		_bodyIndicator.MaxHealth = 6;
		_bodyIndicator.Health = 6;
		_uiso.UpdateIndicator(_health.Value, _shield.Value, null);
		_hand.OnGrabbedItem += OnGrabbedItem;
		_hand.ActivateHand();
		_spawner = FindAnyObjectByType<PlayerSpawner>();
		_weaponContainer.IPDS = IPDS;

		InputSystem.Move += OnMove;
		InputSystem.LeftTrigger += OnLeftTrigger;
		InputSystem.RightTrigger += OnRightTrigger;
		InputSystem.UseItem += OnUseItem;
	}

	public void OnPlayerDespawned()
	{
		_spawnObserver.NotifyPlayerDespawned(this);

		if (!IsOwner)
		{
			_hand.gameObject.SetActive(false);
			_collider.gameObject.SetActive(false);
			_colliderTrigger.gameObject.SetActive(false);
			return;
		}

		_collider.gameObject.SetActive(false);
		_colliderTrigger.gameObject.SetActive(true);

		_health.OnValueChanged -= StatusChanged;
		_shield.OnValueChanged -= StatusChanged;
		_hand.OnGrabbedItem -= OnGrabbedItem;
		_hand.DeactivateHand();

		InputSystem.Move -= OnMove;
		InputSystem.LeftTrigger -= OnLeftTrigger;
		InputSystem.RightTrigger -= OnRightTrigger;
		InputSystem.UseItem -= OnUseItem;
	}

	IEnumerator Burst(float time)
	{
		var t = 0f;
		float t2 = 0f;

		_weaponContainer.ApplyBuff("burst");
		_uiso.UpdateIndicator(
			_health.Value,
			_shield.Value,
			"burst");

		while (t < time)
		{
			if (t2 >= 0.08f)
			{
				IPDS.CreateEffect(
					"EffectBurst",
					transform.position,
					transform.rotation,
					new EffectRpcParameter()
					{
						EffectColor = PersonalColor,
					});
				t2 = 0f;
			}

			t += Time.deltaTime;
			t2 += Time.deltaTime;
			yield return null;
		}

		_weaponContainer.RemoveBuff();
		_buffApplied.Remove("burst");
		_uiso.UpdateIndicator(
			_health.Value,
			_shield.Value,
			"");
	}

	void StatusChanged(int previousValue, int newValue)
	{
		_bodyIndicator.Health = newValue;

		_uiso.UpdateIndicator(
			_health.Value, 
			_shield.Value,
			null);

		if (newValue == 0)
		{
			GLogger.Log($"Player {OwnerClientId} is Dead");
			StopAllCoroutines();
			_colliderTrigger.gameObject.SetActive(false);
			_collider.gameObject.SetActive(false);

			if (IsOwner)
			{
				_hand.DeactivateHand();
				_weaponContainer.Stop();
				_uiso.ShowIndicator(false);
			}
		}
	}

	void OnGrabbedItem(IItemHandler itemHandler)
	{
		if (itemHandler == null)
		{
			_uiso.HideItemPicker();
			_grabbed = false;
		}
		else if (_grabbedItem != itemHandler &&
			_uiso.IsShowingItemPicker())
		{
			// 새로운 item을 잡음
			_uiso.ShowItemPicker(
				itemHandler.GO.transform.position, 
				itemHandler.ItemEffect, 
				itemHandler.IsOnlyFront);
		}

		_grabbedItem = itemHandler;
	}

	void OnGetBuffItem(IItemHandler itemHandler)
	{
		// 효과 즉시 적용
		GLogger.Log($"use buff {itemHandler.ItemEffect}");
		itemHandler.DespawnItemRpc();
	}

	void OnMove(Vector2 direction)
	{
		_inputMovement = direction;
	}

	void OnLeftTrigger(bool trigger)
	{
		_inputLeftTrigger = trigger;
	}

	void OnRightTrigger(bool trigger)
	{
		_inputRightTrigger = trigger;
	}

	void OnUseItem(bool tryPickItem)
	{
		if (tryPickItem && _grabbedItem != null)
		{
			_uiso.ShowItemPicker(
				_grabbedItem.GO.transform.position,
				_grabbedItem.ItemEffect, 
				_grabbedItem.IsOnlyFront);
			_grabbed = true;
			return;
		}
		
		if (_grabbed && !tryPickItem)
		{
			var index = _uiso.GetPickedItemsIndex();
			if (index == 3) // cancel
			{
				_uiso.HideItemPicker();
				return;
			}


			var effect = _grabbedItem.ItemEffect;
			var onlyFront = _grabbedItem.IsOnlyFront;
			

			//GLogger.Log($"Use item at {index} {_grabbedItem.ItemEffect} onlyFront: {_grabbedItem.IsOnlyFront}");
			_weaponContainer.SetWeaponRpc(index, effect);
			_grabbedItem.DespawnItemRpc();
			_uiso.HideItemPicker();
			_grabbed = false;
		}
	}

	[Rpc(SendTo.Owner)]
	void SendCollisionEventRpc(CollisionEventStruct ces)
	{
		//GLogger.Log($"SendCollisionEventRpc sender {ces.SenderId}");
		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ces));
	}

	public void SendCollisionEvent(CollisionEvent ces)
	{
		SendCollisionEventRpc(ces);
		//_collisionEventList.Add(ce);
	}

	public CollisionEvent GetCollisionEvent()
	{
		return _collisionEventCache;
	}
}
