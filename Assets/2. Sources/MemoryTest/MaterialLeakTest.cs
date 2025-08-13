using UnityEditor;
using UnityEngine;

public class MaterialLeakTest : EditorWindow
{
	Material mat;

	[MenuItem("Tools/Material Leak Test")]
	static void Open() => GetWindow<MaterialLeakTest>();

	void OnGUI()
	{
		if (GUILayout.Button("Create Material (No HideFlags)"))
		{
			mat = new Material(Shader.Find("Standard")) // HideFlags 없음
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			Debug.Log($"Created Material: {mat.GetInstanceID()}");
		}

		if (GUILayout.Button("Release Reference"))
		{
			mat = null; // C# 참조 해제
			Debug.Log("Reference cleared (but not destroyed)");
		}

		if (GUILayout.Button("Force GC"))
		{
			System.GC.Collect();
			System.GC.WaitForPendingFinalizers();
			Debug.Log("GC attempted");
		}

		if (GUILayout.Button("Log All Materials in Memory"))
		{
			var mats = Resources.FindObjectsOfTypeAll<Material>();
			Debug.Log($"Materials in memory: {mats.Length}");
			foreach (var m in mats)
				Debug.Log($" - {m.name}, hideFlags: {m.hideFlags}");
		}
	}
}
