using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections.Concurrent;

public class MainThreadDispatcher : MonoBehaviour
{
	//private static MainThreadDispatcher _main;
	private static readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

	public static void Enqueue(Action action)
	{
		if (action == null) return;

		lock (_executionQueue)
		{
			_executionQueue.Enqueue(action);
		}
	}

	void Update()
	{
		lock (_executionQueue)
		{
			while (_executionQueue.Count > 0)
			{
				Action action;
				if (_executionQueue.TryDequeue(out action))
				{
					action();
				}
			}
		}
	}
}
