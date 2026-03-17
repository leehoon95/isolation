using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


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

	NetworkVariable<int> _health = new(
		100, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Owner);
	//NetworkVariable<string> _leftWeaponName = new(
	//	"null",
	//	NetworkVariableReadPermission.Everyone,
	//	NetworkVariableWritePermission.Owner
	//	);
	//NetworkVariable<string> _rightWeaponName = new(
	//	"null",
	//	NetworkVariableReadPermission.Everyone,
	//	NetworkVariableWritePermission.Owner
	//	);
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
	long _lastFiredTime = 0;
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

	public override void OnNetworkSpawn()
	{
		_spawnObserver.NotifyPlayerSpawned(this);
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
		_health.OnValueChanged += HealthChanged;
		_health.Value = 100;
		_hand.OnGrabbedItem += OnGrabbedItem;
		_hand.OnGetBuffItem += OnGetBuffItem;
		_hand.ActiveHand();
		_spawner = FindAnyObjectByType<PlayerSpawner>();
		_weaponContainer.IPDS = IPDS;
		_weaponContainer.PersonalColor = PersonalColor;

		InputSystem.Move += OnMove;
		InputSystem.LeftTrigger += OnLeftTrigger;
		InputSystem.RightTrigger += OnRightTrigger;
		InputSystem.UseItem += OnUseItem;

		_collisionEventCache.SenderId = NetworkObjectId;
	}

	public override void OnNetworkDespawn()
	{
		if (!IsOwner)
		{
			return;
		}
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
			_health.Value -= ce.Damage;
		}

		long now = System.DateTime.Now.Ticks / System.TimeSpan.TicksPerMillisecond;
		if (now - _lastHealthRecoveryTime > 3000)
		{
			_health.Value += 5;

			_lastHealthRecoveryTime = now;
		}

		_health.Value = Mathf.Clamp(_health.Value, 0, 100);

		var mousePosition = (Vector2)Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
		var toMouseVector = mousePosition - (Vector2)transform.position;
		float gunCorrectionAngle = 0f;

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

		_angle = Mathf.Atan2(toMouseVector.y, toMouseVector.x) * Mathf.Rad2Deg + gunCorrectionAngle;

		if (_inputMovement.sqrMagnitude > float.Epsilon)
		{
			var newPosition = _rigidbody.position + _inputMovement.normalized * 5f * Time.fixedDeltaTime;
			_rigidbody.MovePosition(newPosition);
		}

		_weaponContainer.TargetPosition = mousePosition;
		_weaponContainer.Trigger(_inputLeftTrigger);
		if (_inputLeftTrigger)
		{
			
			//if (_inputLeftTrigger && ((now - _lastFiredTime) >= 100))
			//{
			//	var muzzle = mousePosition.normalized;
			//	IPDS.CreateProjectile(
			//		"BulletNormal",
			//		(Vector2)transform.position,
			//		Quaternion.Euler(0f, 0f, _angle + 90),
			//		new ProjectileRpcParameter()
			//		{
			//			StartPosition = transform.position,
			//			TartgetPosition = mousePosition + new Vector2(
			//				UnityEngine.Random.Range(-0.2f, 0.2f), 
			//				UnityEngine.Random.Range(-0.5f, 0.5f)),
			//			CollisionEvent = new CollisionEventStruct()
			//			{
			//				SenderId = NetworkObjectId,
			//				Effect = CollisionEffect.Knockback,
			//				EffectDuration = 0.025f,
			//				EffectIntensity = 1.5f,
			//				Damage = 10
			//			},
			//			EffectColor = _personalColor,
			//			LifeTime = 5f,
			//		});

			//	_lastFiredTime = now;
			//}
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		var ci = collision.gameObject.GetComponentInParent<INetworkObjectCollision>();
		if (ci != null)
		{
			ci.SendCollisionEvent(_collisionEventCache);
		}
	}

	void HealthChanged(int previousValue, int newValue)
	{
		_bodyIndicator.Health = newValue;
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
			

			GLogger.Log($"Use item at {index} {_grabbedItem.ItemEffect} onlyFront: {_grabbedItem.IsOnlyFront}");
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
