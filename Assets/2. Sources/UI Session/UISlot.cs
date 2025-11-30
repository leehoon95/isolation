using TMPro;
using UnityEngine;

public class UISlot : MonoBehaviour
{
    [SerializeField]
    TMP_Text _text;

    public void SetSlotText(string slotName) => _text.text = slotName;
}
