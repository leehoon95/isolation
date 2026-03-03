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

	protected override void Start()
	{
		_targetText = GetComponent<TMP_Text>();
	}

	protected override void OnEnable()
	{
		OnTextPreferredSizeChanged?.Invoke(_targetText.preferredWidth, _targetText.preferredHeight);
	}

	protected override void OnRectTransformDimensionsChange()
	{
		print($"OnRectTransformDimensionsChange {_targetText.preferredWidth}");
		OnTextPreferredSizeChanged?.Invoke(_targetText.preferredWidth, _targetText.preferredHeight); 
	}
}
