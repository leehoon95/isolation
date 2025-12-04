using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

/*
 * scroll rect Àü¿ë
 */

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public class UIChildTextSizeFitter : UIBehaviour
{
    [SerializeField]
    TMP_Text _child;
	[SerializeField]
	TargetSizeAxis _axis;

#if UNITY_EDITOR
	protected override void OnValidate()
	{
		EditorApplication.delayCall += () => FitSize();
	}
#endif

	protected override void OnRectTransformDimensionsChange()
	{
		FitSize();
	}

	public void FitSize()
	{
		var rt = (RectTransform)transform;

		rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _child.preferredHeight);
	}
}
