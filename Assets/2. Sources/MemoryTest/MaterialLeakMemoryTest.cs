using UnityEngine;
using UnityEditor;
using System;
using Unity.VisualScripting;
using System.Collections.Generic;

#if UNITY_EDITOR
public class MaterialLeakMemoryTest : EditorWindow
{
	List<Material> _mat;

	[MenuItem("Tools/Material Leak Memory Test")]
	static void Open() => GetWindow<MaterialLeakMemoryTest>();

	void OnGUI()
	{
		GUILayout.Label("Material Leak Memory Test", EditorStyles.boldLabel);

		if (GUILayout.Button("Create Material (No HideFlags)"))
		{
			var mat = new Material(Shader.Find("Standard"));
			_mat.Add(mat);
			Debug.Log($"Created Material (No HideFlags) ID: {mat.GetInstanceID()}");
			LogMemory();
		}

		if (GUILayout.Button("Create Material (HideAndDontSave)"))
		{
			var mat = new Material(Shader.Find("Standard"))
			{
				hideFlags = HideFlags.HideAndDontSave
			};
			_mat.Add(mat);
			Debug.Log($"Created Material (HideAndDontSave) ID: {mat.GetInstanceID()}");
			LogMemory();
		}

		if (GUILayout.Button("Release Reference"))
		{
			//mat = null; // C# 참조 해제
			_mat.Clear();
			Debug.Log("Reference cleared (not destroyed)");
			LogMemory();
		}

		if (GUILayout.Button("Destroy Material"))
		{
			//if (mat != null)
			//{
			//	DestroyImmediate(mat);
			//	Debug.Log("Material destroyed manually");
			//}
			//else
			//{
			//	Debug.Log("No material to destroy");
			//}
			//mat = null;
			foreach (var mat in _mat)
			{
				DestroyImmediate(mat);

			}

			LogMemory();
		}

		if (GUILayout.Button("Force GC"))
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			Debug.Log("GC attempted");
			LogMemory();
		}

		if (GUILayout.Button("Count All Materials in Memory"))
		{
			var mats = Resources.FindObjectsOfTypeAll<Material>();
			Debug.Log($"Materials in memory: {mats.Length}");
			foreach (var m in mats)
				Debug.Log($" - {m.name}, hideFlags: {m.hideFlags}");
		}
	}

	void LogMemory()
	{
		long totalMemory = GC.GetTotalMemory(false);
		Debug.Log($"[C# Managed Memory] {totalMemory / 1024f:F2} KB");

		// 네이티브 메모리는 Profiler나 Memory Profiler 패키지에서 확인 가능
		// 여기서는 관리 메모리만 출력
	}
}
#endif