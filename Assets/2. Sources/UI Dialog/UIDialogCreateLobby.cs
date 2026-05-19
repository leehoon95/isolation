using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogCreateLobby : UIBehaviour
{
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_InputField _lobbyName;
    [SerializeField] TMP_InputField _lobbyPassword;
    [SerializeField] TMP_Text _content;
    [SerializeField] Button _ok;
    [SerializeField] TMP_Text _okButtonText;

    public event UnityAction<string, string> OnSubmit;

	AudioContainer _ac;

	public string LobbyName
	{
		set => _lobbyName.text = value;
	}

	protected override void Start()
	{
		_ac = AudioContainer.Instance;

		_ok.onClick.AddListener(() => {
			_ac.PlayAudio("click-mouse");
			OnSubmit?.Invoke(_lobbyName.text, _lobbyPassword.text);
			});

//#if UNITY_EDITOR
//        _lobbyName.text = DateTime.Now.ToString();
//#endif
    }

	protected override void OnDisable()
	{
        OnSubmit = null;
	}

	public void SetTitle(string title) => _title.text = title;
    public void SetContent(string content) => _content.text = content;
    public void SetOkButtonText(string text) => _okButtonText.text = text;
}
