using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class UISessionButtons : UIBehaviour
{
    [SerializeField]
    Button _readyButton;
    [SerializeField]
    Button _leaveButton;

    UISessionSO _uiso;

	protected override void Start()
	{
        _uiso = FindAnyObjectByType<UISessionSOHolder>().Data;

		_readyButton.onClick.AddListener(OnClickReady);
		_leaveButton.onClick.AddListener(OnClickLeave);
	}

    void OnClickReady()
    {
        _uiso.RaiseOnClickReady();
    }

    void OnClickLeave()
    {
        _uiso.RaiseOnClickLeave();
    }
}
