using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Rendering;

public class SpwanSystem : ComponentSystem
{
    private const int CHARACTER_COUNT = 100;
    private EntityQuery mGroup = null ;
    public static Stack<Entity> mObjectPool = new Stack<Entity>();
    private int mSpawnCount = 0; 


    protected override void OnUpdate()
    {
        if( mGroup == null )
            mGroup = GetEntityQuery(typeof(SpawnComponent));

        mSpawnCount++;
        if( mGroup.CalculateEntityCount() < CHARACTER_COUNT )
        {
            mSpawnCount = 0;
            spawnCharacter();
        }
    }

    private void spawnCharacter()
    {
        float3 spwanPos = new float3(UnityEngine.Random.Range(0, 100), 3.5f, UnityEngine.Random.Range(0, 100));
        int i;
        for (i = 0; i < 100; i++)
        {
            spwanPos = new float3(UnityEngine.Random.Range(0, 100), 3.5f, UnityEngine.Random.Range(0, 100));
            if (!overlapWithObstacle(spwanPos) && !overlapWithCharacter(spwanPos))
                break;
            Debug.Log("Overlap");
        }
        if (i == 100) return;

        EntityManager entManager = World.Active.EntityManager;
        EntityArchetype entityArchetype = entManager.CreateArchetype(
            typeof(SpawnComponent),
            typeof(MoveComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );
        Entity entity;

        if (mObjectPool.Count == 0)
        {
            entity = entManager.CreateEntity(entityArchetype);

            entManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = Testing.CHARACTER_MESH,
                material = Testing.CHARACTER_MATERIAL
            });
        }
        else
            entity = mObjectPool.Pop();

        entManager.SetComponentData(entity, new Translation { Value = spwanPos });
        entManager.SetComponentData(entity, new MoveComponent { 
            mVelocity = MoveSystem.MOVE_DIRECTION[UnityEngine.Random.Range(0,MoveSystem.MOVE_DIRECTION_COUNT)]*MoveSystem.MOVE_SPEED
        });

    }

    private bool overlapWithObstacle(float3 pos)
    {
        bool rc = false;
        Entities.ForEach((ref ObstacleComponent component, ref Translation translation) =>
        {
            if (System.Math.Abs(pos.x - translation.Value.x) < 0.1f && System.Math.Abs(pos.z - translation.Value.z) < 0.1f)
            {
                rc = true;
                return;
            }
        });
        return rc;
    }
    private bool overlapWithCharacter(float3 pos)
    {
        bool rc = false;
        Entities.ForEach((ref SpawnComponent component, ref Translation translation) =>
        {
            if (System.Math.Abs(pos.x - translation.Value.x) < 1f && System.Math.Abs(pos.z - translation.Value.z) < 1f)
            {
                rc = true;
                return;
            }
        });
        return rc;
    }
}
