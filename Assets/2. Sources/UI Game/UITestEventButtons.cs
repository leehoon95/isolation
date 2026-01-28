using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITestEventButtons : UIBehaviour
{
    UIGameSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UIGameSOHolder>().Data;

		var buttons = GetComponentsInChildren<Button>();

		int index = 0;
		foreach (var button in buttons)
		{
			var i = index;
			button.onClick.AddListener(() => _uiso.RaiseTestEvent(i));
			
			index++;
		}
	}
}
