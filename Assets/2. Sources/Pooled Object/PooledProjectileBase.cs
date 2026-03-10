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

	public bool IsIllusion { get; set; }
	public string PrefabId { get; set; }
	public string ObjectId { get; set; }
	public ulong OwnerClientId { get; set; }
	public GameObject GO => gameObject;
	public IPooledDynamicSpawner Spawner { get => _spawner; set => _spawner = value; }
	public IDynamicPooledObject DPO => this;
	public bool Play { get; set; }

	public virtual void SetTransform(Vector2 position, Quaternion rotation)
	{
		gameObject.transform.SetPositionAndRotation(position, rotation);
	}

	protected virtual void OnDisable()
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}
	}

	public virtual void SetLifeTime(float time = 0)
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}

		_timerCoroutine = StartCoroutine(LifeTimer(time));
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
