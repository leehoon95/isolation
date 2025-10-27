using System;
using UnityEngine;

public static class AwaitableExtensions
{
	public static void Forget(this Awaitable awaitable, string caller = null)
	{
		var awaiter = awaitable.GetAwaiter();

		void HandleException()
		{
			try
			{
				awaiter.GetResult();
			}
			catch (Exception ex) when (ex is OperationCanceledException)
			{
				// Expected cancellation - ignore
			}
			catch (Exception ex)
			{
				if (string.IsNullOrEmpty(caller))
					Debug.LogException(ex);
				else
					Debug.LogError($"[Awaitable Exception in {caller}] {ex}");
			}
		}

		if (awaiter.IsCompleted)
		{
			HandleException();
		}
		else
		{
			awaiter.OnCompleted(HandleException);
		}
	}
}