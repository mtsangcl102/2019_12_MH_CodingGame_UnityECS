using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class MapSystem : ComponentSystem
{
    protected override void OnStartRunning()
    {
        var entityManager = World.Active.EntityManager;

        // Create a map with 100 x 100 x 3 blocks
        var entityArchetype = entityManager.CreateArchetype(
            typeof(Translation) ,
            typeof(RenderMesh) , typeof(LocalToWorld) );

        NativeArray<Entity> entityArray = new NativeArray<Entity>( 30000 , Allocator.Temp );
        entityManager.CreateEntity( entityArchetype , entityArray );

        for ( var x = 0 ; x < 100 ; ++x )
        {
            for ( var y = 0 ; y < 100 ; ++y )
            {
                for ( var z = 0 ; z < 3 ; ++z )
                {
                    var i = z * 10000 + y * 100 + x;
                    var entity = entityArray[i];
                    var height = 0f;

                    // randomize a height
                    if ( z == 0 )
                    {
                        height = UnityEngine.Random.Range( 0 , 100 ) < GameManager.Instance.ObstaclePercentage ? UnityEngine.Random.Range( 1 , 4 ) : 0;
                    }
                    else
                    {
                        var j = y * 100 + x;
                        var translation = entityManager.GetComponentData<Translation>( entityArray[j] );
                        height = translation.Value.y + z;
                    }

                    entityManager.SetComponentData( entity , new Translation { Value = new float3( x , height , y ) } );

                    entityManager.SetSharedComponentData( entity , new RenderMesh()
                    {
                        mesh = GameManager.Instance.Mesh ,
                        material = GameManager.Instance.Materials[ (int) height ]
                    } );
                    
                    // mark it as obstacle
                    if ( Math.Abs( height - 3.0f ) < 0.001f )
                        entityManager.AddComponent( entity , typeof(IsObstacleComponentData) );
                }
            }
        }
    }


    protected override void OnUpdate()
    {
    }
}