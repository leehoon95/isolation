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
/*
 * 자식 오브젝트(Text Mesh Pro)의 가변 크기에 따라 부모 오브젝트 크기를 맞추고 싶을 때 사용함
 * 해당 자식 오브젝트는 UITextSizeNotifier 컴포넌트를 포함해야 함
 */

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
[DisallowMultipleComponent]
public class UITextContentSizeFitter : UIBehaviour
{
	[SerializeField] UITextPreferredSizeNotifier _notifier;
	[SerializeField] float _minWidth;
	[SerializeField] float _minHeight;
	[SerializeField] TargetSizeAxis _targetSizeAxis;

	RectTransform _rt;
	float _textPreferredWidth, _textPreferredHeight;

	protected override void OnEnable()
	{
		base.OnEnable();
		_rt = (RectTransform)transform;
		_notifier.OnTextPreferredSizeChanged += OnTextPreferredSizeChanged;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		_notifier.OnTextPreferredSizeChanged -= OnTextPreferredSizeChanged;
	}

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		EditorApplication.delayCall += () => FitSize();
	}
#endif

	void OnTextPreferredSizeChanged(float preferredWidth, float preferredHeight)
	{
		_textPreferredWidth = preferredWidth;
		_textPreferredHeight = preferredHeight;
		FitSize();
	}

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
				_textPreferredWidth < _minWidth ? _minWidth : _textPreferredWidth);
		}

		if ((_targetSizeAxis | TargetSizeAxis.Vertical) != TargetSizeAxis.None)
		{
			_rt.SetSizeWithCurrentAnchors(
				RectTransform.Axis.Vertical,
				_textPreferredHeight < _minHeight ? _minHeight : _textPreferredHeight);
		}
	}
}
