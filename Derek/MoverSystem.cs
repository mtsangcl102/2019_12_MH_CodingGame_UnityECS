using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

public class MoverSystem : JobComponentSystem
{
    [BurstCompile]
    struct MoveJob : IJobForEach<Translation, ColliderComponent, MoveSpeedComponent>
    {
        [NativeDisableParallelForRestriction][DeallocateOnJobCompletion] public NativeArray<bool> wallMap;
        
        public float deltaTime;
        public Random rand;
        
        public void Execute(ref Translation T, ref ColliderComponent col, ref MoveSpeedComponent m)
        {
            _Move(ref T, ref m, wallMap, deltaTime, rand);
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveJob()
        {
            deltaTime = Time.deltaTime,
            wallMap = new NativeArray<bool>(GameManager.wallMap, Allocator.TempJob),
            rand = new Random((uint) UnityEngine.Random.Range(0, 10000000))
        };
        return job.Schedule(this, inputDependencies);
    }

    private static float3 GetMoveVector(int direction)
    {
        switch (direction)
        {
            case 0: // move to X
                return new float3(5f, 0f, 0f);
            case 1: // move to Z
                return new float3(0f, 0f, 5f);
            case 2: // move to -X
                return new float3(-5f, 0f, 0f);
            case 3:
                return new float3(0f, 0f, -5f);
            default:
                return new float3(5f, 0f, 0f);
        }
    }

    private static void _Move(ref Translation pT, ref MoveSpeedComponent pMsc, NativeArray<bool> wallMap, float deltaTime, Random rand)
    {
        float3 speed = GetMoveVector(pMsc.direction);
        float3 nextStep = pT.Value + new float3(speed.x/5.5f, 0f, speed.z/5.5f);

        int nextX = Mathf.RoundToInt(nextStep.x);
        int nextZ = Mathf.RoundToInt(nextStep.z);
        
        if ((nextX < 0f) || (nextX > 100f) ||
            (nextZ < 0f) || (nextZ > 100f) ||
            wallMap[nextX * 100 + nextZ])
        {
            // pMsc.direction = (pMsc.direction + 1) % 4;
            pMsc.direction = rand.NextInt(0, 4);
            pT.Value.x = (int) pT.Value.x;
            pT.Value.z = (int) pT.Value.z;
            // _Move(ref pT, ref pMsc, wallMap, deltaTime);
        }
        else
        {
            pT.Value += GetMoveVector(pMsc.direction) * deltaTime;
        }
    }

}
