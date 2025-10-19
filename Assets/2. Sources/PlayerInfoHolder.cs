using System;
using Unity.Services.Authentication;
using UnityEditor.U2D;
using UnityEngine;

public class PlayerInfoHolder : MonoBehaviour
{
	static PlayerInfoHolder _instance;

	public static PlayerInfoHolder Instance
	{
		get { return _instance; }
	}

	//[SerializeField]
	//UserInfoSO _template; // Resources
	[SerializeField]
	PlayerInfoSO _playerInfo;


	public PlayerInfoSO PlayerInfo
	{
		get {
			//if (_template != null)
			//{
			//	_userInfo = Instantiate(_template);
			//}
			//else
			if (_playerInfo == null)
			{
				//_template = Resources.Load<UserInfoSO>("UserInfoSO");
				//_userInfo = Instantiate(_template);
				_playerInfo = ScriptableObject.CreateInstance<PlayerInfoSO>();
				//Instantiate
				if (_playerInfo == null)
				{
					throw new NullReferenceException("Loading UserInfo failed");
				}
			}

			return _playerInfo;
		}
		private set => _playerInfo = value;
	}

	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		Debug.Log("PlayerInfoHolder.Awake()");

		
		_instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
