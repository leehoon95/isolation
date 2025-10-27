using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UITextManager : UIBehaviour, INGOTextUI
{
    [SerializeField]
    TMP_Text _text;

	UINGOTestSO _uiso;

	protected override void Start()
	{
		base.Start();

		_uiso = FindAnyObjectByType<UINGOTestSOHolder>().Data;
		_uiso.NGOText = this;
	}

	public void ShowText(string text)
	{
		_text.text = text;
	}
}
