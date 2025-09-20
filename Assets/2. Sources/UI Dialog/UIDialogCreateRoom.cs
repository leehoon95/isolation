using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDialogCreateRoom : UIBehaviour
{
    [SerializeField] TMP_Text _title;
    [SerializeField] TMP_InputField _roomName;
    [SerializeField] TMP_InputField _roomPassword;
    [SerializeField] TMP_Text _content;
    [SerializeField] Button _ok;
    [SerializeField] TMP_Text _okButtonText;

    public event UnityAction<string, string> OnOk;

	protected override void Start()
	{
        _ok.onClick.AddListener(() => OnOk?.Invoke(_roomName.text, _roomPassword.text));
	}

    public void SetTitle(string title) => _title.text = title;
    public void SetContent(string content) => _content.text = content;
    public void SetOkButtonText(string text) => _okButtonText.text = text;
}
