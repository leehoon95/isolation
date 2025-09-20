using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITextManager : UIBehaviour, INGOTextUI
{
    [SerializeField]
    UINGOTestSO _uiso;
    [SerializeField]
    TMP_Text _text;
	protected override void Awake()
	{
		_uiso.NGOText = this;
	}

	public void SetText(string text)
	{
		_text.text = text;
	}
}
