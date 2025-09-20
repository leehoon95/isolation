using UnityEngine;

[CreateAssetMenu(fileName = "UserInfoSO", menuName = "Scriptable Objects/UserInfoSO")]
public class UserInfoSO : ScriptableObject
{
    [SerializeField]
    string _userNickname;
    [SerializeField]
    ulong _token;
    [SerializeField]
    int _roomIndex;
    [SerializeField]
    string _messageFromPreviousScene;
    [SerializeField]
    bool _isHost;
    [SerializeField]
    int _hostingSessionIndex;
    [SerializeField]
    bool _debugging;

    public string UserNickname
    {
        get => _userNickname;
        set => _userNickname = value;
    }

    public ulong Token
    {
        get => _token;
        set => _token = value;
    }

    public int CurrentRoomIndex
    {
        get => _roomIndex;
        set => _roomIndex = value;
    }

    public string MessageFromPreviousScene
    {
        get => _messageFromPreviousScene;
        set => _messageFromPreviousScene = value;
    }

    public bool IsHost
    {
        get => _isHost;
        set => _isHost = value;
    }

    public int HostingSessionIndex
    {
        get => _hostingSessionIndex;
        set => _hostingSessionIndex = value;
	}

    public bool Debugging
    {
        get => _debugging;
        set => _debugging = value;
    }

    //public void SetNickname(string nickname)
    //{
    //    _nickname = nickname;
    //}

    //public string GetNickname()
    //{
    //    return _nickname;
    //}
}
