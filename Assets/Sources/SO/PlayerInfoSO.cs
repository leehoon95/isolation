using UnityEngine;

[CreateAssetMenu(fileName = "PlayerInfoSO", menuName = "Scriptable Objects/PlayerInfoSO")]
public class PlayerInfoSO : ScriptableObject
{
    string _nickname;

    public void SetNickname(string nickname)
    {
        _nickname = nickname;
    }

    public string GetNickname()
    {
        return _nickname;
    }
}
