using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct TargetSearchJob : IJobParallelFor
{
	[ReadOnly] public NativeArray<float3> EnemyPositions;
	[ReadOnly] public NativeArray<float3> PlayerPositions;
	public NativeArray<int> NearestPlayerIndices;

	public void Execute(int index)
	{
		var minDistanceSqr = float.MaxValue;
		int nearestPlayerIndex = -1;

		for (int i = 0; i < PlayerPositions.Length; i++)
		{
			var distanceSq = math.distancesq(
				EnemyPositions[index], PlayerPositions[i]);
			if (distanceSq < minDistanceSqr)
			{
				minDistanceSqr = distanceSq;
				nearestPlayerIndex = i;
			}
		}

		NearestPlayerIndices[index] = nearestPlayerIndex;
	}
}