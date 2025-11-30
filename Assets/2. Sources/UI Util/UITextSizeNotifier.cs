using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class UITextPreferredSizeNotifier : UIBehaviour
{
	TMP_Text _targetText;
	public event Action<float, float> OnTextPreferredSizeChanged;

	protected override void OnEnable()
	{
		base.OnEnable();
		_targetText = GetComponent<TMP_Text>();
	}

	protected override void OnRectTransformDimensionsChange()
	{
		OnTextPreferredSizeChanged?.Invoke(_targetText.preferredWidth, _targetText.preferredHeight); 
	}
}
