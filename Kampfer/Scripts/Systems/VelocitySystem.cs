using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace CodingGame
{
    public class VelocitySystem : JobComponentSystem
    {
        [BurstCompile]
        private struct VelocityJob : IJobForEach<Translation, VelocityComponent>
        {
            public float DeltaTime;
            public void Execute(ref Translation translation, ref VelocityComponent velocity)
            {
                velocity.Value -= new float3(0f, DeltaTime * 60f, 0f);
                translation.Value += ((velocity.Value * DeltaTime));
            }
        }

        
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var job = new VelocityJob()
            {
                DeltaTime = Time.deltaTime
            };
            return job.Schedule(this, inputDependencies);
        }
    }
}