using UnityEngine;

[CreateAssetMenu(fileName = "UserInfoSO", menuName = "Scriptable Objects/UserInfoSO")]
public class PlayerInfoSO : ScriptableObject
{
    [SerializeField]
    string _playerNickname;
    [SerializeField]
    ulong _token;
    [SerializeField]
    int _sessionIndex;
    [SerializeField]
    string _messageFromPreviousScene;
    [SerializeField]
    bool _startHost;
    [SerializeField]
    bool _debugging;
    [SerializeField]
    string _lobbyId;
    [SerializeField]
    string _sessionName;
    [SerializeField]
    string _sessionPassword;
    [SerializeField]
    int _maxPlayers;

    public string PlayerNickname
    {
        get => _playerNickname;
        set => _playerNickname = value;
    }

    public ulong Token
    {
        get => _token;
        set => _token = value;
    }

    public string MessageFromPreviousScene
    {
        get => _messageFromPreviousScene;
        set => _messageFromPreviousScene = value;
    }

    public bool StartHost
    {
        get => _startHost;
        set => _startHost = value;
    }

    public bool Debugging
    {
        get => _debugging;
        set => _debugging = value;
    }

    public string LobbyId
    {
        get => _lobbyId;
        set => _lobbyId = value;
    }

    public string SessionName
    {
        get => _sessionName;
        set => _sessionName = value;
    }

    public string SessionPassword
    {
        get => _sessionPassword;
        set => _sessionPassword = value;
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
