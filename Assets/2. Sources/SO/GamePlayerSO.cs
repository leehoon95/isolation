using UnityEngine;

[CreateAssetMenu(fileName = "GamePlayerSO", menuName = "Scriptable Objects/GamePlayerSO")]
public class GamePlayerSO : ScriptableObject
{
	public bool Play;
}

public class GamePlayerSOHolder : SOHolderSinglton<GamePlayerSO, GamePlayerSOHolder>
{}