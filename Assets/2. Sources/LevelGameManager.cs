using Unity.Cinemachine;
using Unity.Netcode;
using UnityEngine;

public class LevelGameManager : MonoBehaviour
{
	[SerializeField]
	GameProcessor _gameProcessor;

	void Awake()
	{
		if (FindAnyObjectByType<UILevelSOHolder>() == null)
		{
			var obj = new GameObject("[UI Level Holder]");
			obj.AddComponent<UILevelSOHolder>();
		}
	}
}
