using System.Collections;
using UnityEditor.Build.Pipeline;
using UnityEngine;

/*
 * pooling되는 object를 위한 base class
 */
public abstract class PooledProjectileBase : MonoBehaviour, IDynamicPooledObject
{
	IPooledDynamicSpawner _spawner;
	Coroutine _timerCoroutine;

	protected virtual void OnDisable()
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}
	}

	public string PrefabId { get; set; }

	public string ObjectId { get; set; }

	public ulong OwnerClientId { get; set; }

	/*
	 * projectile을 owner client에서만 object 간 상호작용을 해야 한다
	 * 다른 client에는 illusion만 보이는 것이다
	 */
	public bool IsIllusion { get; set; }

	public GameObject GO => gameObject;

	public IPooledDynamicSpawner Spawner { get => _spawner; set => _spawner = value; }

	public IDynamicPooledObject DPO => this;

	public virtual void SetLifeTime(float time = 0)
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}

		_timerCoroutine = StartCoroutine(LifeTimer(time));
	}

	public virtual void SetTransform(Vector2 position, Quaternion rotation)
	{
		gameObject.transform.SetPositionAndRotation(position, rotation);
	}

	IEnumerator LifeTimer(float time)
	{
		yield return new WaitForSeconds(time);
		_timerCoroutine = null;
		Spawner.ReleaseObject(this);
	}

	protected void ReleaseObject()
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}

		Spawner.ReleaseObject(this);
	}	
}
