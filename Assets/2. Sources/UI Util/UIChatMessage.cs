using System;
using System.Globalization;
using TMPro;
using UnityEngine;

public class UIChatMessage : MonoBehaviour
{
	[SerializeField]
	TMP_Text _text;

	string _speakerName = "None";
	string _speakerColorHex = "<color=#FFFFFF>";

	public Color SpeakerColor {
		set => _speakerColorHex = $"<color=#{((int)(value.r)).ToString("X2")}{((int)(value.g )).ToString("X2")}{((int)(value.b )).ToString("X2")}>"; 
		//set => _speakerColorHex = $"<color=#{Convert.ToString(value.r, NumberStyles.HexNumber) }{value.r.ToString("x")}{value.r.ToString("x")}>"; 
	}
	public Color messageColor { set => _text.color = value; }

	public void SetText(string speaker, string text)
	{
		_text.text = $"{_speakerColorHex}{speaker}</color>: {text}";
	}
}
