using UnityEngine;
using WebSocketSharp;

[CreateAssetMenu(fileName = "PlayerInfoSO", menuName = "Scriptable Objects/PlayerInfoSO")]
public class PlayerInfoSO : ScriptableObject
{
    public string Nickname;
    public Color PersonalColor;
    public string LobbyName;
    public string LobbyIdForEntry;
    public string LobbyPassword;
	public bool IsGuestLogin;

    public static Color DeserializePersonalColor(string personalColor)
    {
        if (personalColor.IsNullOrEmpty())
        {
            return Color.white; // invalid color
        }

        var colors = personalColor.Split('/');
        if (colors.Length != 3)
        {
            return Color.white;
        }

        return Color.HSVToRGB(uint.Parse(colors[0]) / 255f, uint.Parse(colors[1]) / 255f, uint.Parse(colors[2]) / 255f);
    }
}

public class PlayerInfoSOHolder : SOHolderSinglton<PlayerInfoSO, PlayerInfoSOHolder>
{}