using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInfoSO", menuName = "Scriptable Objects/PlayerInfoSO")]
public class PlayerInfoSO : ScriptableObject
{
    public string Nickname
    {
        get; set;
    }

    public ulong Token
    {
        get; set;
    }

    public int roomIndex
    {
		get; set;
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
