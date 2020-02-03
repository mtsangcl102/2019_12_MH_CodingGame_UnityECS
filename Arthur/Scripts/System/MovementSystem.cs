using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public class MovementSystem : JobComponentSystem
{
    [BurstCompile]
    struct MovementJob : IJobForEach<Translation, Dat_MovementData>
    {
        public void Execute(ref Translation pos, ref Dat_MovementData mov)
        {
            float2 vec = mov.dir * mov.speed;
            
            pos.Value.x += vec.x;
            pos.Value.z += vec.y;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new MovementJob
        {
            
        };

        return job.Schedule(this, inputDeps);
    }
}