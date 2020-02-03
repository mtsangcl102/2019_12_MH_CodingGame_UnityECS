using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CodingGame
{
    public class InitPlayerSystem : JobComponentSystem
    {
        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }
        
        [RequireComponentTag(typeof(PlayerComponent))]
        [ExcludeComponent(typeof(StartTranslationComponent), typeof(TargetTranslationComponent))]
        private struct InitPlayerJob : IJobForEachWithEntity<InitComponent>
        {
            [ReadOnly]
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            [DeallocateOnJobCompletion] [ReadOnly] 
            public NativeArray<int2> BlockPositionArray;

            public void Execute(Entity entity, int index, [ReadOnly] ref InitComponent initComponent)
            {
                var positionX = initComponent.X;
                var positionY = initComponent.Y;
                var canMove = true;
                for (var i = 0; i < BlockPositionArray.Length; i++)
                {
                    var blockPositionX = BlockPositionArray[i].x;
                    var blockPositionY = BlockPositionArray[i].y;

                    if (positionX != blockPositionX || positionY != blockPositionY) continue;
                    canMove = false;
                    break;
                }

                if (canMove )
                {
                    EntityCommandBuffer.RemoveComponent(index, entity, ComponentType.ReadOnly<InitComponent>());
                    EntityCommandBuffer.AddComponent(index, entity, new StartTranslationComponent(){X = positionX, Y = positionY} );
                }
                else
                {
                    EntityCommandBuffer.DestroyEntity(index, entity);
                }
            }
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var blockEntityQuery = GetEntityQuery(typeof(BlockComponent), ComponentType.ReadOnly<ObstacleComponent>());
            var blockComponentArray = blockEntityQuery.ToComponentDataArray<BlockComponent>(Allocator.TempJob);

            var blockArray = new NativeArray<int2>(blockComponentArray.Length, Allocator.TempJob);
            
            for (var i = 0; i < blockComponentArray.Length; i++)
            {
                blockArray[i] = new int2(blockComponentArray[i].X, blockComponentArray[i].Y);
            }
            
            blockComponentArray.Dispose();

            var initPlayerJob = new InitPlayerJob
            {
                BlockPositionArray = blockArray,
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
            };
            
            var initPlayerJobJobHandle = initPlayerJob.Schedule(this, inputDeps);
            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(initPlayerJobJobHandle);
            
            return initPlayerJobJobHandle;
        }
    }
}