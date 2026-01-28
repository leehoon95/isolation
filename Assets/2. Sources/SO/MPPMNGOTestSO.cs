using UnityEngine;

public enum PlayerIndex : int
{ 
    Player_0 = 0,
    Player_1 = 1,
    Player_2 = 2,
    Player_3 = 3,
}


[CreateAssetMenu(fileName = "MPPMNGOTestSO", menuName = "Scriptable Objects/MPPMNGOTestSO")]
public class MPPMNGOTestSO : ScriptableObject
{
    public bool IsOn;
    public PlayerIndex HostTarget;
}
