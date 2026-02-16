using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class TempProjectile : MonoBehaviour, IDynamicPooledObject
{
	[SerializeField]
	Rigidbody2D _rigidbody;
	[SerializeField]
	Collider2D _collider;
	[SerializeField]
	LayerMask _layerMask;

	string _prefabId;
	string _objectId;
	ulong _ownerClientId;
	IPooledDynamicSpawner _spawner;
	//ProjectileProperty _pp;
	Vector2 _direction;
	float _velocity;
	bool _onlyInteractInOwnerClient;
	Coroutine _coroutineLifetime;
	float _lifeTime;
	Vector2 _startFrom;

	public LayerMask CollisionLayerMask
	{
		get { return _layerMask; }
		set { _layerMask = value; }
	}

	//public ScriptableObject SO
	//{
	//	get => _so;
	//	set
	//	{
	//		var psso = value as ProjectileStatusSO;
	//		if (psso == null)
	//		{
	//			throw new UnityException("Casting failed. ScriptableObject->ProjectileStatusSO");
	//		}

	//		_so = psso;
	//	}
	//}

	/*
	 * IDynamicPooledObject interface 구현
	 */
	public string PrefabId { get => _prefabId; set => _prefabId = value; }
	public string ObjectId { get => _objectId; set => _objectId = value; }
	public ulong OwnerClientId { get => _ownerClientId; set => _ownerClientId = value; }
	public bool IsIllusion { get; set; }

	public IPooledDynamicSpawner Spawner { set => _spawner = value; }
	public IDynamicPooledObject DPO { get => this; }

	public GameObject GO => gameObject;

	public bool OnlyInteractInOwnerClient
	{ 
		get => _onlyInteractInOwnerClient;
		set
		{
			if (value)
			{
				_collider.includeLayers = _layerMask;
			}
			else
			{
				_collider.includeLayers = 0;
			}

			_onlyInteractInOwnerClient = value;
		}
	}

	public Vector2 StartFrom { get => _startFrom; set => _startFrom = value; }

	IEnumerator LifeTimer()
	{
		try
		{
			yield return new WaitForSeconds(_lifeTime);
		}
		finally
		{
			_coroutineLifetime = null;
			Release();
		}
	}

	void Release()
	{
		if (NetworkManager.Singleton.LocalClientId != OwnerClientId)
		{
			return;
		}

		if (_spawner != null)
		{
			_spawner.ReleaseObject(this);
		}
	}

	void OnTriggerEnter2D(Collider2D collision)
	{
		//Debug.Log($"TempProjectile.OnTriggerEnter2D {collision.gameObject.layer} {_collider.includeLayers.value}");
		if (((1 << collision.gameObject.layer) & _layerMask.value) != 0)
		{
			//Debug.Log($"TempProjectile.OnTriggerEnter2D addforce {collision.gameObject.name}");
			var ci = collision.gameObject.GetComponentInParent<IItemHandler>();
			if (ci != null)
			{
				//var force = collision.transform.position - transform.position;
				//ci.AddForce(_rigidbody.linearVelocity.normalized * 1f);

				SetLifeTime(false);
				Release();
			}
		}
	}

	void FixedUpdate()
	{
		//switch (_pp.FlyingType)
		//{
		//	case ProjectileFlyingType.Direct:
		//		_rigidbody.linearVelocity = transform.up * 5f;
		//		break;
		//	case ProjectileFlyingType.Homing:
		//		break;
		//	case ProjectileFlyingType.Registed:
		//		break;
		//}
	}

	public void SetLifeTime(bool active, float time = 0f)
	{
		if (NetworkManager.Singleton.LocalClientId != OwnerClientId)
		{
			return;
		}

		if (active)
		{
			if (_coroutineLifetime == null)
			{
				_lifeTime = time;
				_coroutineLifetime = StartCoroutine(LifeTimer());
			}
		}
		else
		{
			if (_coroutineLifetime != null)
			{
				StopCoroutine(_coroutineLifetime);
				_coroutineLifetime = null;
			}
		}
	}

	public void SetTransform(Vector2 position, Quaternion rotation)
	{
		gameObject.transform.SetPositionAndRotation(position, rotation);
	}

	public void AddForce(Vector2 force)
	{
		// nothing
	}

	public void AddCollisionEvent(CollisionEvent ce)
	{
		throw new System.NotImplementedException();
	}
}
