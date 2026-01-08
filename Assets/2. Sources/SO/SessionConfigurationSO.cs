using UnityEngine;

/*
 * session 설정값을 담은 SO
 * Lobby scene에서 생성하여, Session scene으로 전달한다
 * client는 내부 데이터를 읽고 적절한 행동을 한다
 * 
 */
[CreateAssetMenu(fileName = "SessionConfigureSO", menuName = "Scriptable Objects/SessionConfigureSO")]
public class SessionConfigurationSO : ScriptableObject
{
	/*
	 * Session을 시작하는 client가 Host인지 여부
	 */
	public bool StartAsHost { get; set; }
	public string lobbyId { get; set; }
	public string lobbyName { get; set; }
	public string lobbyPassword { get; set; }
}

public class SessionConfigurationSOHolder : SOHolderSinglton<SessionConfigurationSO, SessionConfigurationSOHolder>
{}