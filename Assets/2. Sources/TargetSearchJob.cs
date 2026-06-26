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
	public NativeArray<int> NearestTargetIndices;

	public void Execute(int index)
	{
		var minDistanceSqr = float.MaxValue;
		int nearestIndex = -1;

		for (int i = 0; i < PlayerPositions.Length; i++)
		{
			var distanceSq = math.distancesq(
				EnemyPositions[index], PlayerPositions[i]);
			if (distanceSq < minDistanceSqr)
			{
				minDistanceSqr = distanceSq;
				nearestIndex = i;
			}
		}

		NearestTargetIndices[index] = nearestIndex;
	}
}