using UnityEngine;

[CreateAssetMenu(fileName = "EnemyInfoSO", menuName = "Scriptable Objects/EnemyInfoSO")]
public class EnemyInfoSO : ScriptableObject
{
	[Header("SuicideBomber")]
	[SerializeField]
	GameObject prefab;
	[SerializeField]
	float Speed;
}
