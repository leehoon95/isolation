using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

[Flags]
enum TargetSizeAxis
{
	None = 0,
	Horizontal = 1,
	Vertical = 2,
}

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class UISizeFitter : UIBehaviour
{
	[SerializeField] RectTransform _rt;
	[SerializeField] RectTransform _targetRT;
	[SerializeField] UISizeNotifier _notifier;
	[SerializeField] float _minWidth;
	[SerializeField] float _minHeight;
	[SerializeField] TargetSizeAxis _targetSizeAxis;

	protected override void Start()
	{
		_notifier.OnRectTransformChanged += FitSize;
	}

	protected override void OnDisable()
	{
		_notifier.OnRectTransformChanged -= FitSize;
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		EditorApplication.delayCall += () => FitSize();
	}
#endif

	void FitSize()
	{
		if (_rt == null)
		{
			return;
		}

		if (_targetSizeAxis == TargetSizeAxis.None)
		{
			return;
		}

		if ((_targetSizeAxis | TargetSizeAxis.Horizontal) != TargetSizeAxis.None)
		{
			_rt.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Horizontal,
				_targetRT.rect.width < _minWidth ? _minWidth : _targetRT.rect.width
				);
		}

		if ((_targetSizeAxis | TargetSizeAxis.Vertical) != TargetSizeAxis.None)
		{
			_rt.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				_targetRT.rect.height >= _minHeight ? _targetRT.rect.height : _minHeight
				);
		}
	}
}
