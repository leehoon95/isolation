using System;
using UnityEngine;
using UnityEngine.EventSystems;

[Flags]
enum TargetSizeAxis
{
	None = 0,
	Horizontal = 1,
	Vertical = 2,
}

[ExecuteInEditMode]
public class UISizeFitter : UIBehaviour
{
	[SerializeField]
	RectTransform _targetRT;
	[SerializeField]
	TargetSizeAxis _targetSizeAxis;

	void Update()
	{
		FitSize();
	}

	void FitSize()
	{
		RectTransform rectTransform = GetComponent<RectTransform>();

		if (rectTransform == null)
		{
			print("This object doesn't have RectTransform.");

			return;
		}

		if (_targetSizeAxis == TargetSizeAxis.None)
		{
			return;
		}

		if ((_targetSizeAxis | TargetSizeAxis.Horizontal) != TargetSizeAxis.None)
		{
			rectTransform.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Horizontal,
				_targetRT.rect.width
				);
		}

		if ((_targetSizeAxis | TargetSizeAxis.Vertical) != TargetSizeAxis.None)
		{
			rectTransform.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				_targetRT.rect.height
				);
		}
	}
}
