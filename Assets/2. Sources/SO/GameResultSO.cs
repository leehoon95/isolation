using UnityEngine;

[CreateAssetMenu(fileName = "GameResultSO", menuName = "Scriptable Objects/GameResultSO")]
public class GameResultSO : ScriptableObject
{
	public int EnemyKilledCount;
	public int PlayerDeadCount;

}

public class GameResultSOHolder : SOHolderSinglton<GameResultSO, GameResultSOHolder>
{ }