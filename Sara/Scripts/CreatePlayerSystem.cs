using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class CreatePlayerSystem : JobComponentSystem
{
    private EntityCommandBuffer _entityCommandBuffer;
    private static EntityArchetype _playerArchetype;

    public struct CreatePlayerJob : IJob
    {
        public EntityCommandBuffer CommandBuffer;
        public Unity.Mathematics.Random Random;
        public int CreateCount;

        public void Execute()
        {
            for (int i = 0; i < CreateCount; i++)
            {
                var entity = CommandBuffer.CreateEntity(_playerArchetype);
                CommandBuffer.SetSharedComponent(entity, new RenderMesh()
                {
                    mesh = SceneManager.GetInstance().CapsuleMesh,
                    material = SceneManager.GetInstance()
                        .PlayerMaterials[Random.NextInt(0, SceneManager.GetInstance().PlayerMaterials.Length)]
                });

                var translation = new Translation
                {
                    Value = new float3(Random.NextInt(0, SceneManager.GetInstance().Width), 3,
                        Random.NextInt(0, SceneManager.GetInstance().Height))
                };
                
                CommandBuffer.SetComponent(entity, translation );
                
                CommandBuffer.SetComponent(entity, new PlayerComponent()
                {
                    TargetPosition = translation.Value
                });
            }
        }
    }

    protected override void OnCreate()
    {
        _playerArchetype = EntityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(PlayerComponent));
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var query = GetEntityQuery(ComponentType.ReadOnly<PlayerComponent>());
        var count = Mathf.Max(SceneManager.GetInstance().PlayerCount - query.CalculateEntityCount(), 0);

        var job = new CreatePlayerJob()
        {
            CommandBuffer = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>().CreateCommandBuffer(),
            CreateCount = count,
            Random = new Unity.Mathematics.Random((uint) UnityEngine.Random.Range(1, 100000))
        };

        var handle = job.Schedule(inputDeps);
        handle.Complete();
        return handle;
    }
}