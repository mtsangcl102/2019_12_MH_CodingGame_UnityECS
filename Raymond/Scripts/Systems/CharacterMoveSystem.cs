using System;
using Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(MapSystem))]
public class CharacterMoveSystem : JobComponentSystem
{
    private EntityQuery _obstacles;
    private EntityQuery _characters;
    
    // private BeginSimulationEntityCommandBufferSystem barrier;

    private NativeArray<Translation> _obstacleTranslations;
    private bool _isInited = false;
    
    protected override void OnCreate()
    {
        // check if it collide with obstacles
        _obstacles = GetEntityQuery(ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<IsObstacleComponentData>());
        // _characters = GetEntityQuery(ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<MoveComponentData>());
        //
        // barrier = World.GetOrCreateSystem<BeginSimulationEntityCommandBufferSystem>();
    }
    
    
    [BurstCompile]
    struct CharacterMoveSystemJob : IJobForEachWithEntity<Translation, MoveComponentData, NewPositionComponentData, IsDestroyedComponentData>
    {
        public float DeltaTime;
        
        // [DeallocateOnJobCompletion]
        // [ReadOnly] public NativeArray<Translation> Characters;
        [ReadOnly] public NativeArray<Translation> Obstacles;
        // [ReadOnly] public EntityCommandBuffer EntityCommandBuffer;
        
        public void Execute( Entity entity , int index , [ReadOnly] ref Translation translation, ref MoveComponentData moveComponentData , ref NewPositionComponentData newPositionComponentData , ref IsDestroyedComponentData isDestroyedComponentData )
        {
            // translation.Value += moveComponentData.Value * DeltaTime * 10 ;
            var newPosition = translation.Value + moveComponentData.Value * DeltaTime * 20 ;
            
            var isCollided = false;
            
            for ( var i = 0 ; i < Obstacles.Length ; ++ i )
            {
                var obstacle = Obstacles[i];
                
                if ( isCollided )
                    break;
                
                if ( Math.Abs( obstacle.Value.x - newPosition.x ) < 1.0f &&
                     Math.Abs( obstacle.Value.z - newPosition.z ) < 1.0f )
                {
                    // do not move if collided with obstacle, and simply rotate by 90
                    moveComponentData.Value = new float3( moveComponentData.Value.z , moveComponentData.Value.y , - moveComponentData.Value.x);

                    isCollided = true;
                }
            }
          
            
            // collide with the boundary
            if ( !isCollided )
            {
                if ( newPosition.x < 0.5f || newPosition.x > 98.5f ||
                     newPosition.z < 0.5f || newPosition.z > 98.5f )
                {
                    // do not move if collided with obstacle, and simply rotate by 90
                    moveComponentData.Value = new float3( moveComponentData.Value.z , moveComponentData.Value.y , - moveComponentData.Value.x);

                    isCollided = true;
                }
            }
            
            if ( !isCollided )
            {
                newPositionComponentData.Value = newPosition;
            }
            else
            {
                newPositionComponentData.Value = translation.Value;
            }
            
            // // if collide with character
            // for ( var i = 0 ; i < Characters.Length ; ++i )
            // {
            //     var character = Characters[i];
            //
            //     if ( i == index )
            //         continue;
            //     
            //     if ( SceneHelper.IsCollide( character.Value , newPosition ) )
            //     {
            //         EntityCommandBuffer.DestroyEntity( entity );
            //         break;
            //     }
            // }
        }

    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new CharacterMoveSystemJob();

        if ( !_isInited )
        {
            _isInited = true;
            _obstacleTranslations = _obstacles.ToComponentDataArray<Translation>( Allocator.Persistent );
        }
        
        job.DeltaTime = UnityEngine.Time.deltaTime;
        job.Obstacles = _obstacleTranslations;
        // job.Characters = _characters.ToComponentDataArray<Translation>( Allocator.TempJob );
        // job.EntityCommandBuffer = barrier.CreateCommandBuffer();
        
        // Debug.Log( $"character length: {job.Characters.Length}" );
        
        // job.Obstacles = new NativeArray<Translation>( _obstacles.ToComponentDataArray<Translation>( Allocator.TempJob ) , Allocator.TempJob );
        
        // // Now that the job is set up, schedule it to be run. 
        // inputDependencies = job.Schedule(this , inputDependencies);
        //
        // JobHandle.ScheduleBatchedJobs();
        // inputDependencies.Complete();
        // barrier.AddJobHandleForProducer(inputDependencies);
        // return inputDependencies;
        //
        // inputDependencies = job.Schedule(this , inputDependencies);
        // inputDependencies.Complete();
        // return inputDependencies;

        return job.Schedule( this , inputDependencies );
    }
    
    
    
}