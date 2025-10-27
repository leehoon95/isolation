using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[InitializeOnLoad]
[ExecuteInEditMode]
public class MemoryCleaner
{
    static MemoryCleaner()
    {
        EditorApplication.update -= Update;
        EditorApplication.update += Update;

    }

    static void Update()
    {
        if (EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            ClearMemory();
        }
    }

    static void ClearMemory()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        Debug.Log("Memory cleaned atfer exiting play mode.");
    }
}
#endif