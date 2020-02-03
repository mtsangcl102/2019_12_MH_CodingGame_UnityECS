using System.ComponentModel.Design;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class UpdatePlayerSystem : JobComponentSystem
{
    private EntityCommandBuffer _entityCommandBuffer;
    private static EntityArchetype _playerArchetype;
    private EntityQueryDesc query;
    
    public struct UpdatePlayerJob : IJobForEachWithEntity<PlayerComponent, Translation>
    {
        [ReadOnly]
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public Unity.Mathematics.Random Random;
        [DeallocateOnJobCompletionAttribute][NativeDisableParallelForRestriction]
        public NativeArray<Entity> Entities;
        [DeallocateOnJobCompletionAttribute][NativeDisableParallelForRestriction]
        public NativeArray<Translation> Translations;
        
        public void Execute(Entity entity, int index, ref PlayerComponent playerComponent, ref Translation translation)
        {
            if (!SceneManager.GetInstance().IsInited) return;
            var map = SceneManager.GetInstance().Map;

            // Check Collision
            var x = translation.Value.x;
            var z = translation.Value.z;
            
            if (SceneManager.GetInstance().Map == null || float.IsNaN(x) || float.IsNaN(z) )
                return;
            
            if (map[(int) x][(int) z] != ObstacleType.Floor)
            {
                CommandBuffer.DestroyEntity(index, entity);
                return;
            }

            for (int i = 0; i < Entities.Length; i++)
            {
                if (!Entities[i].Equals(entity))
                {
                    if (math.length(Translations[i].Value - translation.Value) < 0.5f)
                    {
                        CommandBuffer.DestroyEntity(index, entity);
                        return; 
                    }
                }
            }

            // Check Target Reached
            if ( math.abs(x - playerComponent.TargetPosition.x) < 0.5f && math.abs(z -playerComponent.TargetPosition.z) < 0.5f)
            {
                if (Random.NextBool())
                {
                    playerComponent.TargetPosition.x = Random.NextInt(0, SceneManager.GetInstance().Width);                    
                }
                else
                {
                    playerComponent.TargetPosition.z = Random.NextInt(0, SceneManager.GetInstance().Height);
                }

            }
            
            // Move!
            translation.Value += math.normalize(playerComponent.TargetPosition - translation.Value) * SceneManager.GetInstance().Speed;
        }
    }

    protected override void OnCreate()
    {
        base.OnCreate();
        query = new EntityQueryDesc {All = new ComponentType[] {typeof(PlayerComponent), typeof(Translation)}};
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
//        var query = GetEntityQuery(ComponentType.ReadOnly<PlayerComponent>());
//        var count = Mathf.Max(SceneManager.GetInstance().PlayerCount - query.CalculateEntityCount(), 0);

        
        var group = GetEntityQuery(query);
        var entities = group.ToEntityArray(Allocator.TempJob);
        var translations = group.ToComponentDataArray<Translation>(Allocator.TempJob);
        
        var job = new UpdatePlayerJob()
        {
            CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer().ToConcurrent(),
            Random = new Unity.Mathematics.Random((uint) UnityEngine.Random.Range(1, 100000)),
            Entities = entities,
            Translations = translations,
        };

        var handle = job.Schedule(this, inputDeps);
        handle.Complete();
        return handle;
    }
}