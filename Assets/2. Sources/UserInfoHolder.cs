using System;
using UnityEditor.U2D;
using UnityEngine;

public class UserInfoHolder : MonoBehaviour
{
	static UserInfoHolder _instance;

	public static UserInfoHolder Instance
	{
		get { return _instance; }
	}

	[SerializeField]
	UserInfoSO _template; // Resources
	[SerializeField]
	UserInfoSO _userInfo;


	public UserInfoSO UserInfo
	{
		get {
			if (_template != null)
			{
				_userInfo = Instantiate(_template);
			}
			else
			{
				var origin = Resources.Load<UserInfoSO>("UserInfoSO");

				_userInfo = Instantiate(origin);

				if (_userInfo == null)
				{
					throw new NullReferenceException("Loading UserInfo failed");
				}
			}

			return _userInfo;
		}
		private set => _userInfo = value;
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Debug.Log("UserInfoHolder.Awake()");

		
		_instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
