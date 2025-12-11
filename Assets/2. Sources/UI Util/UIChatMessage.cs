using TMPro;
using UnityEngine;

public class UIChatMessage : MonoBehaviour
{
	[SerializeField]
	TMP_Text _text;

	public string text { set => _text.text = value; }
	public Color textColor { set => _text.color = value; }
}
