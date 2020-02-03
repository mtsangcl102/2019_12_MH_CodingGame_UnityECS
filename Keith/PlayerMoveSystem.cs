using System.Collections;
using System.Collections.Generic;
using System ;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Jobs ;
using Unity.Collections ;
using Unity.Burst ;
using Random = UnityEngine.Random;

public class PlayerMoveSystem : JobComponentSystem
{
    private EntityQuery group;
    EntityCommandBufferSystem m_EndFrameBarrier;
    
    [BurstCompile]
    public struct Task : IJobForEachWithEntity<Translation, PlayerComponent>
    {
        public float deltaTime;
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<Translation> translations;
        [DeallocateOnJobCompletion]
        [ReadOnly]
        public NativeArray<bool> treeMap;
        public EntityCommandBuffer.Concurrent CommandBuffer;
        //[ReadOnly] public uint BaseSeed;
        public Unity.Mathematics.Random random;
        //public EntityQuery group;
        //public NativeList<float2> positionList;
        public void Execute( Entity entity, int index, ref Translation translation, ref PlayerComponent playerComponent)
        {
            //NativeArray<Translation> translations = group.ToComponentDataArray<Translation>(Allocator.Temp);
            for (int i = 0; i < translations.Length; i++)
            {
                if ( i == index )
                {
                    continue;
                }
                if ( Mathf.RoundToInt(translations[i].Value.x) == Mathf.RoundToInt(translation.Value.x) && Mathf.RoundToInt(translations[i].Value.y) == Mathf.RoundToInt(translation.Value.y))
                {
                    CommandBuffer.DestroyEntity( index, entity );
                    return;
                }
            }
            Move(ref translation, ref playerComponent);
        }
        private void Move( ref Translation translation, ref PlayerComponent playerComponent )
        {
            int x, y;
            switch (playerComponent.direction)
            {
                case 0 :
                    x = 0;
                    y = 1;
                    break;
                case 1 :
                    x = 1;
                    y = 0;
                    break;
                case 2 :
                    x = 0;
                    y = -1;
                    break;
                default :
                    x = -1;
                    y = 0;
                    break;
            }
            int targetX = Mathf.RoundToInt(translation.Value.x) + x;
            int targetY = Mathf.RoundToInt(translation.Value.y) + y;
            if ( targetX < 0 || targetX >= 100 || targetY < 0 || targetY >= 100 
                 ||treeMap[ targetX + targetY * 100 ] )
            {
                //var rnd = new Unity.Mathematics.Random(BaseSeed);
                playerComponent.direction = random.NextInt(0, 4);
                //playerComponent.direction = (playerComponent.direction + 1) % 4;
                translation.Value = new float3( Mathf.RoundToInt(translation.Value.x), Mathf.RoundToInt(translation.Value.y), translation.Value.z );
                Move(ref translation, ref playerComponent);
            }
            else
            {
                translation.Value += new float3( x, y, 0f ) * 5f * deltaTime;
            }
        }
    }

    protected override void OnCreate()
    {
        group = GetEntityQuery(typeof(Translation), typeof(PlayerComponent));
        m_EndFrameBarrier = World.GetOrCreateSystem<EntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        //group.SetFilter(new WorldComponent {isPlayer = true});
        var job = new Task() { 
            CommandBuffer = m_EndFrameBarrier.CreateCommandBuffer().ToConcurrent(), 
            translations = group.ToComponentDataArray<Translation>(Allocator.TempJob), 
            deltaTime = Time.deltaTime,
            treeMap =  new NativeArray<bool>( ECSGameManager.treeMap, Allocator.TempJob ),
            random = new Unity.Mathematics.Random( (uint)Random.Range( 1, 10000 ) )
        };
        JobHandle handle = job.Schedule(this, inputDependencies);
        m_EndFrameBarrier.AddJobHandleForProducer(handle);
        return handle;
    }
}
