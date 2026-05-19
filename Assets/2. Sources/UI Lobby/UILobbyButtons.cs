using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UILobbyButtons : UIBehaviour, IUILobbyButtons
{
	[SerializeField]
	Button _createSessionButton;
	//[SerializeField]
	//Button _settingButton;
	[SerializeField]
	Button _refreshButton;
	[SerializeField]
	Button _exitButton;
	[SerializeField]
	CanvasGroup _canvasGroup;

	UILobbySO _uiso;
	AudioContainer _ac;
	protected override void Start()
	{
		_ac = AudioContainer.Instance;
		_uiso = FindAnyObjectByType<UILobbySOHolder>().Data;
		_uiso.Buttons = this;

		_createSessionButton.onClick.AddListener(() => { _ac.PlayAudio("click-mouse"); _uiso.RaiseOnClickCreateLobby(); });
		_refreshButton.onClick.AddListener(() => { _ac.PlayAudio("click-mouse"); _uiso.RaiseOnClickRefresh(); });
		_exitButton.onClick.AddListener(() => { _ac.PlayAudio("click-mouse"); _uiso.RaiseOnClickExit(); });
	}

	public void SetInteractable(bool interactable)
	{
		_canvasGroup.interactable = interactable;
	}
}
