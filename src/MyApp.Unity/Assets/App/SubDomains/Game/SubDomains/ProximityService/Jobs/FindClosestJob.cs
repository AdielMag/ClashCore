using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace App.SubDomains.Game.SubDomains.ProximityService.Jobs
{
    [BurstCompile]
    public struct FindClosestExcludingJob : IJobParallelFor
    {
        [ReadOnly] public float3 source;
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public float rangeSq;
        [ReadOnly] public int selfIndex;

        [NativeDisableParallelForRestriction]
        public NativeArray<int> results;
        [NativeDisableParallelForRestriction]
        public NativeArray<float> minDistSq;

        public void Execute(int index)
        {
            if (index == selfIndex) return;

            float distSq = math.lengthsq(positions[index] - source);
            if (distSq <= rangeSq && distSq < minDistSq[0])
            {
                minDistSq[0] = distSq;
                results[0]   = index;
            }
        }
    }

}