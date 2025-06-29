using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace App.SubDomains.Game.SubDomains.ProximityService.Jobs
{
    [BurstCompile]
    public struct FindAllInRangeExcludingJob : IJobParallelFor
    {
        [ReadOnly] public float3 source;
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public float rangeSq;
        [ReadOnly] public int selfIndex;

        [WriteOnly] public NativeArray<float> distances;

        public void Execute(int index)
        {
            if (index == selfIndex)
            {
                distances[index] = -1f;
                return;
            }

            float distSq = math.lengthsq(positions[index] - source);
            distances[index] = (distSq <= rangeSq) ? distSq : -1f;
        }
    }

}