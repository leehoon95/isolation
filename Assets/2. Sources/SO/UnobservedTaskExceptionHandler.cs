using System.Threading.Tasks;
using UnityEngine;

[CreateAssetMenu(fileName = "UnobservedTaskExceptionHandler", menuName = "Scriptable Objects/UnobservedTaskExceptionHandler")]
public class UnobservedTaskExceptionHandler : ScriptableObject
{
	public void Handler(object sender, UnobservedTaskExceptionEventArgs e)
	{
		GLogger.LogError($"UnobservedTaskException ¹ß»ý\n{e.Exception.InnerException.Message}");
		e.SetObserved();
	}
}

public class UnobservedTaskExceptionHandlerHolder 
	: SOHolderSinglton<UnobservedTaskExceptionHandler, UnobservedTaskExceptionHandlerHolder>
{
	protected override void Awake()
	{
		base.Awake();
	}
}