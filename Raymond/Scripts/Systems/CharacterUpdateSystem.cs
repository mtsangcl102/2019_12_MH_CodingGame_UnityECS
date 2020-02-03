using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(CharacterMoveSystem))]
public class CharacterUpdateSystem : JobComponentSystem
{
    [BurstCompile]
    struct CharacterUpdateSystemJob : IJobForEach<Translation, NewPositionComponentData>
    {
        public void Execute(ref Translation translation, [ReadOnly] ref NewPositionComponentData newPositionComponentData)
        {
            translation.Value = newPositionComponentData.Value;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new CharacterUpdateSystemJob();
        
        // Now that the job is set up, schedule it to be run. 
        return job.Schedule(this, inputDependencies);
    }
}