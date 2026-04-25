using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIGameResult : UIBehaviour, IUIGameResult
{
	[SerializeField]
	TMP_Text _result;
	[SerializeField]
	Button _exitToLobbyButton;

	UIGameResultSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UIGameResultSOHolder>().Data;
		_uiso.GameResult = this;

		_exitToLobbyButton.onClick.AddListener(() => {
			_uiso.RaiseOnExitToLobby();
		});
	}

	public void SetResult(int result)
	{
		_result.text = result.ToString();
	}

	public void ShowExitToLobbyButton()
	{
		_exitToLobbyButton.gameObject.SetActive(true);
	}
}
