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
    public class MovePlayerSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }
        
        [RequireComponentTag(typeof(PlayerComponent))]
        [ExcludeComponent(typeof(StartTranslationComponent))]
        [BurstCompile]
        private struct MovePlayerBurstJob : IJobForEach<Translation, TargetTranslationComponent, MoveSpeedComponent>
        {
            public void Execute(ref Translation playerTranslation, [ReadOnly] ref TargetTranslationComponent targetTranslation, [ReadOnly] ref MoveSpeedComponent moveSpeed)
            {
                var newPosition = new float2(playerTranslation.Value.x + moveSpeed.Value * targetTranslation.Direction.x,playerTranslation.Value.z + moveSpeed.Value * targetTranslation.Direction.y);
                    
                if (targetTranslation.Direction.x > 0)
                {
                    newPosition.x = math.min(newPosition.x, targetTranslation.X);
                }
                else if( targetTranslation.Direction.x < 0)
                {
                    newPosition.x =  math.max(newPosition.x, targetTranslation.X);
                }
                    
                if (targetTranslation.Direction.y > 0)
                {
                    newPosition.y = math.min(newPosition.y, targetTranslation.Y);
                }
                else if( targetTranslation.Direction.y < 0)
                {
                    newPosition.y = math.max(newPosition.y, targetTranslation.Y);
                }
                    
                playerTranslation.Value = new float3(newPosition.x, playerTranslation.Value.y, newPosition.y);
            }
        }
        
        [RequireComponentTag(typeof(PlayerComponent))]
        [ExcludeComponent(typeof(StartTranslationComponent))]
        private struct ResetPlayerJob : IJobForEachWithEntity<Translation, TargetTranslationComponent>
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;

            public void Execute(Entity entity, int index, ref Translation playerTranslation, [ReadOnly] ref TargetTranslationComponent targetTranslation)
            {
                var currentPosition = new float2(playerTranslation.Value.x, playerTranslation.Value.z);
                var targetPosition = new float2(targetTranslation.X, targetTranslation.Y);

                if (math.distance(currentPosition, targetPosition) <= 0f)
                {
                    EntityCommandBuffer.AddComponent(index, entity, new StartTranslationComponent(){X = targetTranslation.X, Y = targetTranslation.Y} );
                    EntityCommandBuffer.RemoveComponent( index, entity, ComponentType.ReadOnly<TargetTranslationComponent>());
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var movePlayerBurstJob = new MovePlayerBurstJob();
            var movePlayerJobHandle = movePlayerBurstJob.Schedule(this, inputDeps);

            var resetPlayerJob = new ResetPlayerJob
            {
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            
            var resetPlayerJobHandle = resetPlayerJob.Schedule(this, movePlayerJobHandle);
            
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(resetPlayerJobHandle);
            
            return resetPlayerJobHandle;
        }
    }
}