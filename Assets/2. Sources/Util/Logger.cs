using System;
using UnityEngine;


public sealed class GLogger
{
    const string _ENABLE_LOG = "ENABLE_LOG";

	public static bool IsDebugBuild() => Debug.isDebugBuild;

	[System.Diagnostics.Conditional(_ENABLE_LOG)]
	public static void Log(string message)
    {
        Debug.Log(message);
    }

	[System.Diagnostics.Conditional(_ENABLE_LOG)]
	public static void LogWarning(string message)
    {
		Debug.LogWarning(message);
	}

    [System.Diagnostics.Conditional(_ENABLE_LOG)]
	public static void LogError(string message)
	{
		Debug.LogError(message);
	}

	[System.Diagnostics.Conditional(_ENABLE_LOG)]
	public static void LogException(Exception e)
	{
		Debug.LogException(e);
	}

	[System.Diagnostics.Conditional(_ENABLE_LOG)]
	public static void LogException(Exception e, UnityEngine.Object o)
	{
		Debug.LogException(e, o);
	}
}
