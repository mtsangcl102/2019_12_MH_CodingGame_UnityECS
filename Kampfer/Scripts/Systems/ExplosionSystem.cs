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
    public class ExplosionSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }

        
        [RequireComponentTag(typeof(ExplosionComponent))]
        [BurstCompile]
        private struct BecomeSmallerBrustJob : IJobForEach<Scale>
        {
            public float DeltaTime;
            public void Execute(ref Scale scale)
            {
                scale.Value -= (scale.Value * DeltaTime);
            }
        }
        
        [RequireComponentTag(typeof(ExplosionComponent))]
        private struct KillJob : IJobForEachWithEntity<Translation>
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;

            public void Execute(Entity entity, int index, [ReadOnly] ref Translation translation)
            {
                if (translation.Value.y < 0f)
                {
                    EntityCommandBuffer.DestroyEntity( index, entity);
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDependencies)
        {
            var becomeSmallerBurstJob = new BecomeSmallerBrustJob()
            {
                DeltaTime = Time.deltaTime
            };

            var becomeSmallerBurstJobHandle = becomeSmallerBurstJob.Schedule(this, inputDependencies);


            var killJob = new KillJob()
            {
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            
            var killJobHandle = killJob.Schedule(this, becomeSmallerBurstJobHandle);
            
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(killJobHandle);
            
            return killJobHandle;
        }
    }
}