using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

// deprecated
public class NetworkBootLoader : MonoBehaviour
{
	[SerializeField]
	string SceneNameToLoad;

	// 게임 실행(Play) 버튼을 누를 때마다 호출됨
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
	static void ResetNetworkManager()
	{
		// 이전 세션의 Singleton 참조를 초기화하여 
		// 새로운 NetworkManager가 Singleton을 정상적으로 점유하도록 함
		if (NetworkManager.Singleton != null)
		{
			Object.DestroyImmediate(NetworkManager.Singleton.gameObject);
		}
	}

	private void Awake()
	{
		var networkManager = GetComponent<NetworkManager>();

		// 1. 이미 다른 Singleton이 존재하고 현재 자신과 다르다면 이전 것은 제거
		if (NetworkManager.Singleton != null && NetworkManager.Singleton != networkManager)
		{
			// 도메인 리로드가 안되면 이전 세션의 NetworkManager가 'Missing' 상태로 남을 수 있음
			Destroy(NetworkManager.Singleton.gameObject);
		}

		// 2. 수동으로 NetworkManager 셋업 (필요한 경우)
		// 보통 Awake에서 자동으로 수행되지만, NGO 툴과의 충돌을 방지하기 위해 
		// 씬 로드 직후 NetworkConfig가 유효한지 확인합니다.
		//if (networkManager.NetworkConfig == null)
		//{
		//	Debug.LogError("NetworkConfig가 설정되지 않았습니다!");
		//}
	}

	void Start()
	{
		SceneManager.sceneLoaded += SceneLoaded;
		SceneManager.LoadScene(SceneNameToLoad);
	}

	void OnDestroy()
	{
		SceneManager.sceneLoaded -= SceneLoaded;
	}

	void SceneLoaded(Scene scene, LoadSceneMode mode)
	{
		GLogger.Log($"Network Boot Loader Scene Loaded {scene}. {mode}");
	}
}
