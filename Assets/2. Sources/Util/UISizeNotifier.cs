using System;
using UnityEngine;
using UnityEngine.EventSystems;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class UISizeNotifier : UIBehaviour
{
	public event Action OnRectTransformChanged;

	protected override void OnRectTransformDimensionsChange()
	{
		OnRectTransformChanged?.Invoke();
	}
}
