using UnityEngine;

public class UserInfoHolder : MonoBehaviour
{
	static UserInfoHolder _instance;

	public static UserInfoHolder Instance
	{
		get { return _instance; }
	}

	[SerializeField]
	UserInfoSO _original;

	UserInfoSO _userInfo;


	public UserInfoSO UserInfo
	{
		get => _userInfo;
		private set => _userInfo = value;
	}



	void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);
	}
}
