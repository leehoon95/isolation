using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;


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
	SpriteRenderer _bodySprite;
	[SerializeField]
	PlayerHand _hand;
	[SerializeField]
	PlayerCameraTarget _cameraTarget;
	[SerializeField]
	SortingGroup _sortingGroup;
	[Header("Spec")]
	[SerializeField]
	float _speedNormal;
	[SerializeField]
	float _speedBursted;
	[SerializeField]
	bool _invincible;
	[SerializeField]
	int _maxHealth;

	NetworkVariable<int> _health = new(
		6, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Owner);
	NetworkVariable<int> _shield = new(
		0,
		NetworkVariableReadPermission.Everyone,
		NetworkVariableWritePermission.Owner);
	List<CollisionEvent> _collisionEventList = new();
	CollisionEvent _collisionEventCache = new()
	{
		Position = Vector2.zero,
		Direction = Vector2.right,
		Effect = CollisionEffect.Block,
		EffectDuration = 0f,
		Damage = 0,
	};
	IWeaponInterface _leftWeapon;
	IWeaponInterface _rightWeapon;
	UILevelSO _uiso;
	IItemHandler _grabbedItem;
	bool _grabbed;
	Vector2 _inputMovement;
	bool _inputLeftTrigger;
	bool _inputRightTrigger;
	float _speed;
	Color _personalColor;
	float _angle;
	long _lastHealthRecoveryTime;
	Dictionary<string, Coroutine> _buffApplied = new();
	AudioContainer _ac;
	long _automaticAttackEndTick;
	long _automaticRelaxEndTick;

	public string Nickname { get; set; }

	public Color PersonalColor 
	{
		get => _personalColor;
		set
		{
			_personalColor = value;
			_bodySprite.color = value;
			_weaponContainer.PersonalColor = value;
		}
	}

	public InputSystem InputSystem { get; set; }
	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IPlayerSpawner Spawner { get; set; }
	public Transform CameraTarget => _cameraTarget.transform;
	public IPooledDynamicSpawner IPDS { get; set; }
	public Vector3 SpawnPosition { get; set; }
	public ulong SpawnClientId { get; set; }
	public bool AutomaticMotion { get; set; }

	public override void OnNetworkSpawn()
	{
		GLogger.Log($"pp OnNetworkSpawn owner id: {OwnerClientId}, isOwner: {IsOwner}");
		Spawner.NotifyPlayerSpawned(this);
		if (OwnerClientId == SpawnClientId)
		{
			OnPlayerSpawned();
		}
	}

	public override void OnNetworkDespawn()
	{
		OnPlayerDespawned();
	}

	protected override void OnOwnershipChanged(ulong previous, ulong current)
	{
		GLogger.Log($"pp OnOwnershipChanged {previous} to {current}. IsOwner: {IsOwner}");
		//if (IsOwner)
		{
			OnPlayerSpawned();
		}
	}

	void Update()
	{
		if (!IsOwner)
		{
			return;
		}

		if (_uiso.IsShowingItemPicker())
		{
			_uiso.MoveItemPicket(_grabbedItem.GO.transform.position);
		}

		_uiso.UpdateIndicatorPosition(transform.position);

		if (AutomaticMotion)
		{
			return;
		}

		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var toMouseVector = mousePosition - (Vector2)transform.position;
		_angle = Mathf.Atan2(toMouseVector.y, toMouseVector.x) * Mathf.Rad2Deg;
		_weaponContainer.TargetPosition = mousePosition;

		transform.rotation = Quaternion.Euler(0f, 0f, _angle);
	}

	void FixedUpdate()
	{
		if (!IsOwner)
		{
			return;
		}

		if (AutomaticMotion)
		{
			var ph = Spawner.GetPlayer(0);
			if (ph == null)
			{
				return;
			}

			var direction = ph.GO.transform.position - transform.position;
			var hostPlayerRotation = ph.GO.transform.rotation;
			var distance = direction.magnitude;

			if (distance > 2f)
			{
				var newPosition = _rigidbody.position + (Vector2)direction.normalized * _speed * Time.fixedDeltaTime;
				_rigidbody.MovePosition(newPosition);
			}

			_angle = hostPlayerRotation.eulerAngles.z;
			transform.rotation = Quaternion.Euler(0f, 0f, _angle);
			_weaponContainer.TargetPosition = transform.position + (hostPlayerRotation * Vector3.right) * 10f;
		}

		while (_collisionEventList.Count > 0)
		{
			var ce = _collisionEventList[0];
			_collisionEventList.RemoveAt(0);
			//GLogger.Log($"hit {ce.SenderId}/{ce.Effect}/{ce.Position}");
			_ac.PlayAudio("hit-2");
			if (_shield.Value > 0)
			{
				_shield.Value -= ce.Damage > 0 ? 1 : 0;
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
		if (now - _lastHealthRecoveryTime > 5000)
		{
			_health.Value = Mathf.Min(_health.Value + 1, 6);

			_lastHealthRecoveryTime = now;
		}

		if (AutomaticMotion)
		{
			if (_inputLeftTrigger && now - _automaticRelaxEndTick > 3000f)
			{
				_inputLeftTrigger = false;
				_automaticAttackEndTick = now;
			}
			else if (!_inputLeftTrigger && now - _automaticAttackEndTick > 1000f)
			{
				_inputLeftTrigger = true;
				_automaticRelaxEndTick = now;
			}
		}

		_weaponContainer.Trigger(_inputLeftTrigger);

		if (_inputMovement.sqrMagnitude > float.Epsilon)
		{
			var newPosition = _rigidbody.position + _inputMovement.normalized * _speed * Time.fixedDeltaTime;
			_rigidbody.MovePosition(newPosition);
			//_rigidbody.linearVelocity = _inputMovement.normalized * _speed;
		}
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
			if (itemHandler.ItemType == ItemType.Weapon)
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
				IPDS.CreateEffectLocal(
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
		GLogger.Log($"OnPlayerSpawned initialize {NetworkManager.LocalClientId}");
		
		_ac = AudioContainer.Instance;

		if (!IsOwner)
		{
			_hand.gameObject.SetActive(false);
			//_collider.gameObject.SetActive(false);
			_colliderTrigger.gameObject.SetActive(false);
			return;
		}
		
		_collisionEventCache.SenderId = NetworkObjectId;

		_collider.gameObject.SetActive(true);
		_colliderTrigger.gameObject.SetActive(true);
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;

		_health.Value = _maxHealth;
		_shield.Value = 0;
		_health.OnValueChanged += StatusChanged;
		_shield.OnValueChanged += StatusChanged;
		_uiso.UpdateIndicator(_health.Value, _shield.Value, null);

		_hand.gameObject.SetActive(true);
		_hand.OnGrabbedItem += OnGrabbedItem;
		_hand.ActivateHand();
		_weaponContainer.IPDS = IPDS;
		_speed = _speedNormal;

		InputSystem.Move += OnMove;
		InputSystem.LeftTrigger += OnLeftTrigger;
		InputSystem.UseItem += OnUseItem;
	}

	public void OnPlayerDespawned()
	{
		Spawner.NotifyPlayerDespawned(this);

		if (!IsOwner)
		{
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
		_speed = _speedBursted;

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

		_speed = _speedNormal;
		_weaponContainer.RemoveBuff();
		_buffApplied.Remove("burst");
		_uiso.UpdateIndicator(
			_health.Value,
			_shield.Value,
			"");
	}

	void StatusChanged(int previousValue, int newValue)
	{
		
		_uiso.UpdateIndicator(
			_health.Value, 
			_shield.Value,
			null);

		if (_health.Value == 0)
		{
			GLogger.Log($"Player {OwnerClientId} is Dead");
			_collider.gameObject.SetActive(false);
			_colliderTrigger.gameObject.SetActive(false);
		
			_hand.DeactivateHand();
			_weaponContainer.Stop();
			IPDS.CreateEffect(
				"EffectPopBig",
				transform.position,
				Quaternion.identity,
				new EffectRpcParameter()
				{
					EffectColor = PersonalColor
				});
			StartCoroutine(ProcessPlayerDeadEvent());
		}
	}

	IEnumerator ProcessPlayerDeadEvent()
	{
		yield return null;
		Spawner.SpawnPlayerDeadBodyRpc(
			transform.position,
			transform.rotation,
			Nickname,
			PersonalColor);
	
		Spawner.DespawnPlayerRpc();
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

	void OnUseItem(bool tryPickItem)
	{
		if (tryPickItem && _grabbedItem != null)
		{
			_uiso.ShowItemPicker(
				_grabbedItem.GO.transform.position,
				_grabbedItem.ItemEffect, 
				_grabbedItem.IsOnlyFront);
			_grabbed = true;
			_ac.PlayAudio("get-item");
			
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

			if (_grabbedItem.ItemType == ItemType.Weapon)
			{
				_ac.PlayAudio("get-weapon");
			}
			else
			{
				_ac.PlayAudio("chutter-click");
			}
				
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
		if (_invincible)
		{
			return;
		}

		_collisionEventList.Add(new CollisionEvent().FromCollisionEventStruct(ces));
	}

	public void SendCollisionEvent(CollisionEvent ces)
	{
		SendCollisionEventRpc(ces);
	}

	public CollisionEvent GetCollisionEvent()
	{
		return _collisionEventCache;
	}
}
