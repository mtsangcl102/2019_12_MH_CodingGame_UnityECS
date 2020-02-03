using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;


[UpdateBefore(typeof(MovementSystem))]
public class CollisionSystem : JobComponentSystem
{
    EntityQuery mapTileGroup;
    EntityQuery playerGroup;

    private EntityCommandBufferSystem endFrameBuffer;
    private EntityCommandBuffer.Concurrent currentBuffer;

    protected override void OnCreate()
    {
        endFrameBuffer = World.GetOrCreateSystem<EntityCommandBufferSystem>();
        
        playerGroup = GetEntityQuery(ComponentType.ReadOnly<Translation>(), typeof(Dat_MovementData));
        mapTileGroup = GetEntityQuery(ComponentType.ReadOnly<Dat_MapTileData>());
    }
    
    [BurstCompile]
    public struct CollisionJob : IJobForEachWithEntity<Translation, Dat_MovementData>
    {
        public EntityCommandBuffer.Concurrent currentBuffer;
        
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Dat_MapTileData> tileDataToTestAgainst;
        [DeallocateOnJobCompletion]
        [ReadOnly] public NativeArray<Translation> playerDataToTestAgainst;
        
        public void Execute(Entity entity, int index, ref Translation t, ref Dat_MovementData m)
        {
            Random rand = new Random((uint)index + 1);
            
            int2 currentPos = new int2((int)math.round(t.Value.x), (int)math.round(t.Value.z));
            int posID = currentPos.x + GameManager.mapSize * currentPos.y;

            int2 nextPos = (int2)(currentPos + m.dir);
            int nextPosID = nextPos.x + GameManager.mapSize * nextPos.y;

            for(int i = 0; i < playerDataToTestAgainst.Length; i++)
            {
                int2 pos = new int2((int)math.round(playerDataToTestAgainst[i].Value.x), (int)math.round(playerDataToTestAgainst[i].Value.z));
                if (pos.x == currentPos.x && pos.y == currentPos.y && i != index)
                {
                    currentBuffer.DestroyEntity(index, entity);
                    return;
                }
            }


            int count = 5;
            while ((nextPos.x < 0 || nextPos.x > GameManager.mapSize - 1 || nextPos.y < 0 || nextPos.y > GameManager.mapSize - 1 || tileDataToTestAgainst[nextPosID].isObstacle) && count > 0)
            {
                float ran = rand.NextFloat();

                if (ran < 0.3f)//rt right
                {
                    float temp = m.dir.y;
                    m.dir.y = m.dir.x;
                    m.dir.x = temp;
                }else if (ran < 0.6f)
                {
                    float temp = m.dir.y;
                    m.dir.y = m.dir.x;
                    m.dir.x = temp;
                    m.dir *= -1f;
                }
                else
                {
                    m.dir *= -1;
                }
                
                nextPos = (int2)(currentPos + m.dir);
                nextPosID = nextPos.x + GameManager.mapSize * nextPos.y;

                count--;
            }

            if (tileDataToTestAgainst[posID].isObstacle || count <= 0)
            {
                currentBuffer.DestroyEntity(index, entity);
            }
        }
    }

    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var jobPvE = new CollisionJob()
        {
            currentBuffer = endFrameBuffer.CreateCommandBuffer().ToConcurrent(),
            tileDataToTestAgainst = mapTileGroup.ToComponentDataArray<Dat_MapTileData>(Allocator.TempJob),
            playerDataToTestAgainst = playerGroup.ToComponentDataArray<Translation>(Allocator.TempJob),
        };

        return jobPvE.Schedule(this, inputDependencies);
    }
}
