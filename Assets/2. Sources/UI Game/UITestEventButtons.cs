using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UITestEventButtons : UIBehaviour
{
    UILevelSO _uiso;

	protected override void Start()
	{
		_uiso = FindAnyObjectByType<UILevelSOHolder>().Data;

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
