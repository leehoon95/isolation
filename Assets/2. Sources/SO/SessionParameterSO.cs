using UnityEngine;

[CreateAssetMenu(fileName = "SessionParameterSO", menuName = "Scriptable Objects/SessionParameterSO")]
public class SessionParameterSO : ScriptableObject
{
    public string LobbyId;
    public string LobbyName;
    public string LobbyPassword;
    public int MaxPlayers;
}

public class SessionParameterSOHolder : SOHolderSinglton<SessionParameterSO, SessionParameterSOHolder>
{}