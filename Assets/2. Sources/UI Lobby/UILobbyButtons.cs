using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobbyButtons : UIBehaviour
{
	[SerializeField]
	Button _createSessionButton;
	[SerializeField]
	Button _settingButton;
	[SerializeField]
	Button _refreshButton;
	[SerializeField]
	Button _exitButton;

	UILobbySO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;

		_createSessionButton.onClick.AddListener(() => _uiso.RaiseOnClickCreateSession());
		_settingButton.onClick.AddListener(() => _uiso.RaiseOnClickSettings());
		_refreshButton.onClick.AddListener(() => _uiso.RaiseOnClickRefresh());
		_exitButton.onClick.AddListener(() => _uiso.RaiseOnClickExit());
	}
}
