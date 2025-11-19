using NUnit.Framework.Constraints;
using System.Collections;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.Schema;
using TMPro.EditorUtilities;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Pool;

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
	ulong _clientId;
	IPooledDynamicSpawner _spawner;
	ProjectileStatusSO _so;
	Vector2 _direction;
	float _velocity;
	bool _onlyInteractInOwnerClient;
	Coroutine _coroutineLifetime;

	public LayerMask CollisionLayerMask
	{
		get { return _layerMask; }
		set { _layerMask = value; }
	}

	public ScriptableObject SO
	{
		get => _so;
		set
		{
			var psso = value as ProjectileStatusSO;
			if (psso == null)
			{
				throw new UnityException("Casting failed. ScriptableObject->ProjectileStatusSO");
			}

			_so = psso;
		}
	}
	public IPooledDynamicSpawner Spawner { set => _spawner = value; }
	public IDynamicPooledObject DPO { get { return this; } }

	public GameObject GO => gameObject;

	public NetworkObject NO => null;

	public bool OnlyInteractInOwnerClient
	{ 
		get => _onlyInteractInOwnerClient;
		set
		{
			if (value)
			{
				_collider.includeLayers = _layerMask.value;
			}
			else
			{
				_collider.includeLayers = 0;
			}

			_onlyInteractInOwnerClient = value;
		}
	}

	public string PrefabId { get => _prefabId; set => _prefabId = value; }
	public string ObjectId { get => _objectId; set => _objectId = value; }
	public ulong ClientId { get => _clientId; set => _clientId = value; }

	IEnumerator LifeTimer()
	{
		try
		{
			yield return new WaitForSeconds(3f);
		}
		finally
		{
			_coroutineLifetime = null;
			Release();
		}
	}

	void Release()
	{
		if (NetworkManager.Singleton.LocalClientId != ClientId)
		{
			return;
		}

		if (_spawner != null)
		{
			_spawner.ReleaseObject(this);
		}
	}

	void OnTriggerEnter(Collider other)
	{
		Debug.Log("TempProjectile.OnTriggerEnter");
		//if (!HasAuthority)
		//{
		//	return;
		//}

		if ((other.gameObject.layer & _layerMask.value) != 0)
		{
			Debug.Log($"TempProjectile collided layer: {other.gameObject.layer}");
			//_pool.Release(this);
		}
	}

	void FixedUpdate()
	{
		switch (_so.FlyingType)
		{
			case ProjectileFlyingType.Direct:
				_rigidbody.linearVelocity = transform.up * _so.Velocity;
				break;
		}
	}

	public void SetActive(bool active)
	{
		if (NetworkManager.Singleton.LocalClientId != ClientId)
		{
			return;
		}

		gameObject.SetActive(active);

		if (active)
		{
			if (_coroutineLifetime == null)
			{
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

	public void Clean()
	{
	}
}
