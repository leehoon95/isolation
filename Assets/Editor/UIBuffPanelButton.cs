using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UIBuffPanel))]
public class UIBuffPanelButton : Editor
{
	string _buffName;
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.Space();

		_buffName = EditorGUILayout.TextField(_buffName);

		UIBuffPanel m = (UIBuffPanel)target;
		if (GUILayout.Button("Add Buff"))
		{
			m.AddBuff(_buffName);
		}

		if (GUILayout.Button("Remove Buff"))
		{
			m.RemoveBuff();
		}

		if (GUILayout.Button("Log"))
		{
			m.Log();
		}
	}
}
