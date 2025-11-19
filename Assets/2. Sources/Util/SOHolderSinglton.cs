using System;
using UnityEngine;

/*
 * scene 범위에 싱글턴을 구현하면서 
 * Multiplayer Play Mode의 가상 player 마다 런타임 인스턴스를 생성하기 위함 base class
 * DontDestroyOnLoad는 GameManager가 결정
 */
public abstract class SOHolderSinglton<T, TS> : MonoBehaviour 
	where T : ScriptableObject
	where TS : SOHolderSinglton<T, TS>
{
	public static TS Instance { get; private set; }
	protected T RuntimeInstance;
	public T Data => RuntimeInstance;

	protected virtual void Awake()
	{
		if (Instance != null && Instance != null)
		{
			Destroy(gameObject);
			return;
		}

		Instance = this as TS;
		if (Instance == null)
		{
			throw new NullReferenceException("SOHolderSinglton.Awake Instance is null");
		}

		RuntimeInstance = ScriptableObject.CreateInstance<T>();
	}
}
