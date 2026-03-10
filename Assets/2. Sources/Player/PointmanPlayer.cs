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
	GameObject _leftWeaponPosition;
	[SerializeField]
	GameObject _rightWeaponPosition;
	[SerializeField]
	WeaponListSO _weaponList;
	[SerializeField]
	SpriteRenderer _bodySprite;
	[SerializeField]
	PlayerBodyIndicator _bodyIndicator;
	[SerializeField]
	PlayerHand _hand;
	[SerializeField]
	PlayerCameraTarget _cameraTarget;

	InputSystem _inputSystem;
	PooledDynamicSpawner _pds;
	NetworkVariable<int> _health = new(
		100, 
		NetworkVariableReadPermission.Everyone, 
		NetworkVariableWritePermission.Owner);
	IPlayerSpawnObserver _spawnObserver;
	IWeaponInterface _leftWeapon;
	IWeaponInterface _rightWeapon;
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
		set
		{
			_personalColor = value;
			_bodyIndicator.PersonalColor = value;
		}
	}
	public InputSystem InputSystem { set => _inputSystem = value; }
	public NetworkObject NO => NetworkObject;
	public GameObject GO => gameObject;
	public IPlayerSpawnObserver SpawnObserver { set => _spawnObserver = value; }
	public Transform CameraTarget => _cameraTarget.transform;

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
		
		_inputSystem = FindAnyObjectByType<InputSystem>();
		_pds = FindAnyObjectByType<PooledDynamicSpawner>();

		if (_inputSystem == null
			|| _pds == null)
		{
			throw new NullReferenceException($"Check null reference"
				+ $"input system is {_inputSystem}\n"
				+ $"pooled dynamic spawner is {_pds}");
		}

		_collider.gameObject.SetActive(true);
		_colliderTrigger.gameObject.SetActive(true);
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;
		_health.OnValueChanged += HealthChanged;
		_health.Value = 100;
		_hand.OnGrabbedItem += OnGrabbedItem;
		_hand.ActiveHand();
		_spawner = FindAnyObjectByType<PlayerSpawner>();
		
		if (_uiso == null)
		{
			GLogger.LogError("uiso is null");
		}

		_inputSystem.Move += OnMove;
		_inputSystem.LeftTrigger += OnLeftTrigger;
		_inputSystem.RightTrigger += OnRightTrigger;
		_inputSystem.UseItem += OnUseItem;

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

		//if (_leftWeapon != null)
		{
			if (_inputLeftTrigger && ((now - _lastFiredTime) >= 100))
			{
				var muzzle = mousePosition.normalized;
				_pds.CreateProjectile(
					"BulletNormal",
					(Vector2)transform.position,
					Quaternion.Euler(0f, 0f, _angle + 90),
					new ProjectileRpcParameter()
					{
						FlyingType = ProjectileFlyingType.Homing,
						StartPosition = transform.position,
						TartgetPosition = mousePosition + new Vector2(
							UnityEngine.Random.Range(-0.2f, 0.2f), 
							UnityEngine.Random.Range(-0.5f, 0.5f)),
						Speed = 12f,
						SpeedDeltaPerSec = 0f,
						MaxAngularVelocity = 720f,
						CollisionEvent = new CollisionEventStruct()
						{
							SenderId = NetworkObjectId,
							Effect = CollisionEffect.Knockback,
							EffectDuration = 0.025f,
							EffectIntensity = 1.5f,
							Damage = 10
						},
						EffectColor = _personalColor,
						LifeTime = 5f,
					});

				_lastFiredTime = now;
			}
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
			_uiso.ShowItemPicker(itemHandler.GO.transform.position, itemHandler.ItemEffect, itemHandler.IsOnlyFront);
		}
		_grabbedItem = itemHandler;
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
			_uiso.ShowItemPicker(_grabbedItem.GO.transform.position, _grabbedItem.ItemEffect, _grabbedItem.IsOnlyFront);
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
