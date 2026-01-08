using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInfoSO2", menuName = "Scriptable Objects/PlayerInfoSO2")]
public class PlayerInfoSO2 : ScriptableObject
{
    [SerializeField]
    string _playerName;
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
    string _lobbyName;
    [SerializeField]
    bool _createLobbyWithPassword;
    [SerializeField]
    string _lobbyPassword;
    [SerializeField]
    int _maxPlayers;

    public string PlayerName
    {
        get => _playerName;
        set => _playerName = value;
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

    //public bool Debugging
    //{
    //    get => _debugging;
    //    set => _debugging = value;
    //}

    public string LobbyIdForEntry
    {
        get => _lobbyId;
        set => _lobbyId = value;
    }

    public string LobbyName
    {
        get => _lobbyName;
        set => _lobbyName = value;
    }

    public bool CreateLobbyWithPassword
    {
        get => _createLobbyWithPassword;
        set => _createLobbyWithPassword = value;
    }

    public string LobbyPassword
    {
        get => _lobbyPassword;
        set => _lobbyPassword = value;
    }
}

public class PlayerInfoHolder2 : SOHolderSinglton<PlayerInfoSO2, PlayerInfoHolder2>
{}