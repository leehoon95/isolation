using System;
using UnityEngine;

/*
 * scene에 scriptable object를 싱글턴 오브젝트로 존재
 * Multiplayer Play Mode의 가상 player 마다 런타임 인스턴스를 생성하기 위한 base class
 * DontDestroyOnLoad는 해당 object를 참조하는 GameManager가 결정할 것
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
			DestroyImmediate(gameObject);
			return;
		}

		Instance = this as TS;
		if (Instance == null)
		{
			throw new NullReferenceException("SOHolderSinglton.Awake Instance is null");
		}
		
		RuntimeInstance = ScriptableObject.CreateInstance<T>();
	}

	/*
	 * 필드 모두 GC대상으로 에디터, 빌드에서 안전하나 명시적 파괴 필요시 참고할 것
	 */
	//protected virtual void OnDestroy()
	//{
	//	if (RuntimeInstance != null)
	//	{
	//		Destroy(RuntimeInstance);
	//	}

	//	if (Instance == this)
	//	{
	//		Instance = null;
	//	}
	//}
}
