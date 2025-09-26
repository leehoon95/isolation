using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISessionList))]
public class SessionListButtons : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UISessionList component = (UISessionList)target;
		if (GUILayout.Button("Add Temp Session"))
		{
			component.AddTempSession();
		}
	}
}
