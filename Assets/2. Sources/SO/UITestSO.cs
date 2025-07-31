using UnityEngine;

[CreateAssetMenu(fileName = "UITestSO", menuName = "Scriptable Objects/UITestSO")]
public class UITestSO : ScriptableObject
{
    DebugGameObject _debugGameObject;

	public void SetDebugGameObject(DebugGameObject debugGameObject)
	{
		_debugGameObject = debugGameObject;
	}

	public void SetDebugText(string text)
	{
		if (_debugGameObject != null)
		{
			_debugGameObject.SetDebugText(text);
		}
		else
		{
			Debug.LogWarning("DebugGameObject is not set in UITestSO.");
		}
	}
}
