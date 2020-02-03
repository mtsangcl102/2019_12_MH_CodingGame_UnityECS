using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace CodingGame
{
    public class PlayerCollisionSystem : JobComponentSystem
    {
        private struct EntityWithPosition {
            public Entity Entity;
            public float3 Position;
        }

        private EndSimulationEntityCommandBufferSystem _endSimulationEntityCommandBufferSystem;
        protected override void OnCreate()
        {
            _endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
            base.OnCreate();
        }
        
        [RequireComponentTag(typeof(PlayerComponent))]
        [BurstCompile]
        private struct PlayerCollisionBurstJob : IJobForEachWithEntity<Translation>
        {
            [DeallocateOnJobCompletion] [ReadOnly] 
            public NativeArray<EntityWithPosition> PlayerEntityArray;
            public NativeArray<Entity> CollidedEntityArray;
 
            public void Execute(Entity entity, int index, [ReadOnly] ref Translation playerTranslation)
            {
                var collidedEntity = Entity.Null;
                for (var i = 0; i < PlayerEntityArray.Length; i++)
                {
                    if (PlayerEntityArray[i].Entity.Index == entity.Index || math.distance(playerTranslation.Value, PlayerEntityArray[i].Position) > 0.5f) continue;
                    collidedEntity = PlayerEntityArray[i].Entity;
                    break;
                }

                CollidedEntityArray[index] = collidedEntity;
            }
        }

        private struct HandleCollisionJob : IJobForEachWithEntity<PlayerComponent, Translation> 
        {
            public EntityCommandBuffer.Concurrent EntityCommandBuffer;
            
            [DeallocateOnJobCompletion] [ReadOnly] public NativeArray<Entity> CollidedEntityArray;

            public void Execute(Entity entity, int index, [ReadOnly] ref PlayerComponent playerComponent, [ReadOnly] ref Translation playerTranslationComponent) {
                if (CollidedEntityArray[index] != Entity.Null) {
                    EntityCommandBuffer.DestroyEntity(index, entity);

                    var explosionCenterEntity = EntityCommandBuffer.CreateEntity(index);
                    EntityCommandBuffer.AddComponent(index, explosionCenterEntity, new ExplosionCenterComponent
                    {
                        Position = playerTranslationComponent.Value, 
                        MaterialIndex = playerComponent.MaterialIndex
                    });
                }
            }
        }
        
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var playerQuery = GetEntityQuery(typeof(PlayerComponent), ComponentType.ReadOnly<Translation>());
            var playerEntities = playerQuery.ToEntityArray(Allocator.TempJob);
            var translationComponents = playerQuery.ToComponentDataArray<Translation>(Allocator.TempJob);

            var playerTranslationArray = new NativeArray<EntityWithPosition>(translationComponents.Length, Allocator.TempJob);

            for (var i = 0; i < translationComponents.Length; i++)
            {
                playerTranslationArray[i] = new EntityWithPosition {
                    Entity = playerEntities[i],
                    Position = translationComponents[i].Value,
                };
            }
            
            playerEntities.Dispose();
            translationComponents.Dispose();

            var collidedEntityArray = new NativeArray<Entity>(playerQuery.CalculateEntityCount(), Allocator.TempJob);
            var playerCollisionBurstJob = new PlayerCollisionBurstJob
            {
                PlayerEntityArray = playerTranslationArray,
                CollidedEntityArray = collidedEntityArray,
            };

            var handleCollisionJob = new HandleCollisionJob
            {
                CollidedEntityArray = collidedEntityArray,
                EntityCommandBuffer = _endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
            };
            
            var playerCollisionBurstJobHandle = playerCollisionBurstJob.Schedule(this, inputDeps);
            var handleCollisionJobHandle = handleCollisionJob.Schedule(this, playerCollisionBurstJobHandle);

            _endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(handleCollisionJobHandle);

            return handleCollisionJobHandle;
        }
    }
}