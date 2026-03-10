using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(UISessionCommunication))]
public class UISessionCommunicationButton : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		UISessionCommunication uisc = (UISessionCommunication)target;
		if (GUILayout.Button("Send Message"))
		{
			uisc.TestChatMessage();
		}
	}
}
