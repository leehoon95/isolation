using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EffectWave))]
public class EffectWaveButton : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		EffectWave ew = (EffectWave)target;
		if (GUILayout.Button("Start Wave"))
		{
			ew.StartWaveCoroutin();
		}
	}
}
