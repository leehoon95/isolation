using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MaintainPPU))]
public class MaintainPPUEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		MaintainPPU m = (MaintainPPU)target;
		if (GUILayout.Button("Maintain PPU"))
		{
			m.Maintain();
		}
	}
}
