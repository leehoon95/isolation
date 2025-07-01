using TMPro;
using UnityEngine;

public class DebugGameObject : MonoBehaviour
{
    [SerializeField]
	UITestSO _uiTestSO;
	[SerializeField]
    TMP_Text _debugText;

	void Start()
	{
		_uiTestSO.SetDebugGameObject(this);
	}

	public void SetDebugText(string text)
	{
		_debugText.text = text;
	}
}
