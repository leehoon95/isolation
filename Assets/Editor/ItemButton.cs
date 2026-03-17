using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		Item m = (Item)target;
		if (GUILayout.Button("Refresh"))
		{
			m.RefreshItemShape();
		}
	}
}
