using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPlayerSlot : UIBehaviour
{
    [SerializeField]
    GameObject _slotParent;
    [SerializeField]
    GameObject _slotPrefab;
    [SerializeField]
    List<UISlot> _slots;

	protected override void OnRectTransformDimensionsChange()
	{
		
	}

	public void AddPlayerSlot(string slotName)
    {

    }
}
