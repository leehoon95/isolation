using UnityEditor;
using UnityEngine;

public class CacheCleanerMenu
{
	[MenuItem("Custom/Clean Cache")]
	private static void DoSomething()
	{
		Debug.Log("Clear Cache");
		Caching.ClearCache();
	}
}
