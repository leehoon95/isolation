using NUnit.Framework.Internal;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UILobbyList))]
public class LobbySessionListButtons : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EditorGUILayout.LabelField("Test", EditorStyles.boldLabel);

		UILobbyList component = (UILobbyList)target;
		
		//if (Selection.objects.Length > 0 )
		//{
		//	for (int i = 0; i < Selection.objects.Length; i++)
		//	{
		//		EditorGUILayout.LabelField($"{Selection.objects[i].name}");
		//	}
			
		//}
		
		if (GUILayout.Button("Add Temp Session"))
		{
			component.AddTempSession();
		}

		if (GUILayout.Button("Add Temp Session 10"))
		{
			component.AddTempSession_10();
		}

		if (GUILayout.Button("Clear All Session"))
		{
			component.ClearSessionList();
		}
	}
}
