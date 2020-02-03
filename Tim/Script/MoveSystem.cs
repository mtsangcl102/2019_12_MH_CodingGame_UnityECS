using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;

public class MoveSystem : ComponentSystem
{
    public static readonly float3[] MOVE_DIRECTION = { new Vector3(1,0,0), new Vector3(0,0,1), new Vector3(-1,0,0), new Vector3(0,0,-1) };
    public static readonly int MOVE_DIRECTION_COUNT = 4;
    public static readonly float MOVE_SPEED = 20.0f;

    protected override void OnUpdate()
    {
        EntityManager entManager = World.Active.EntityManager;

        EntityQuery entityQuery = GetEntityQuery(typeof(MoveComponent),typeof(Translation));
        var eArray = entityQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var moveArray = entityQuery.ToComponentDataArray<MoveComponent>(Unity.Collections.Allocator.TempJob);
        NativeArray<Translation> translationArray = entityQuery.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);

        for ( int i=0; i<eArray.Length; i++ )
        {
            Entity ent = eArray[i];
            MoveComponent moveComponent = moveArray[i];
            Translation translation = entManager.GetComponentData<Translation>(ent) ;
            float3 newPos = (moveComponent.mVelocity * Time.deltaTime) + translation.Value;

            if( CollideWithObstacle(newPos,ent,typeof(SpawnComponent)))
            {
                entManager.SetEnabled(ent, false);
                SpwanSystem.mObjectPool.Push(ent);
                continue;
            }

            // check if outside level or collide with obstalce
            if (IsOutSideLevel(newPos, 99f) || CollideWithObstacle(newPos,ent,typeof(ObstacleComponent)) )
            {
                //random another velocity
                moveComponent.mVelocity = MOVE_DIRECTION[UnityEngine.Random.Range(0, MOVE_DIRECTION_COUNT)] * MOVE_SPEED;
                entManager.SetComponentData<MoveComponent>(ent, moveComponent);
            }
            else
                entManager.SetComponentData<Translation>(ent, new Translation { Value = newPos });
        }//);

        eArray.Dispose();
        moveArray.Dispose();
        translationArray.Dispose();
    }

    private bool IsOutSideLevel(float3 pos, float level_size)
    {
        return (pos.x < 0 || pos.z < 0 || pos.x > level_size || pos.z > level_size);
    }

    // Checks if the square at position posA and size sizeA overlaps 
    // with the square at position posB and size sizeB
    private bool AreSquaresOverlapping(float3 posA, float sizeA, float3 posB, float sizeB)
    {
        float d = (sizeA / 2) + (sizeB / 2);
        return (math.abs(posA.x - posB.x) < d && math.abs(posA.z - posB.z) < d);
    }

    private bool CollideWithObstacle(float3 pos, Entity entity, ComponentType componentType)
    {
        EntityQuery entityQuery = GetEntityQuery(componentType, typeof(Translation));

        var eArray = entityQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
        var translationArray = entityQuery.ToComponentDataArray<Translation>(Unity.Collections.Allocator.TempJob);

        bool rc = false;
        for (int i = 0; i < eArray.Length; i++)
        {
            Entity ent = eArray[i];
            Translation translation = translationArray[i];

            if (ent == entity)
                continue;

            if (AreSquaresOverlapping(pos, 1, translation.Value, 1))
            {
                rc = true;
                break;
            }
        }

        eArray.Dispose();
        translationArray.Dispose();
        return rc;
    }
}