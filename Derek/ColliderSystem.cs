using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class ColliderSystem : ComponentSystem
{
    private int j = 0;
    protected override void OnUpdate()
    {
        NativeArray<Translation> playerT = GetEntityQuery(typeof(PlayerComponent), typeof(Translation)).ToComponentDataArray<Translation>(Allocator.TempJob);
        NativeArray<Entity> players = GetEntityQuery(typeof(PlayerComponent), typeof(Translation)).ToEntityArray(Allocator.TempJob);

        Entities.ForEach((Entity e, ref MoveSpeedComponent move, ref Translation t) =>
                         {
                             for (int i = 0; i < playerT.Length; i++)
                             {
                                 // Player detection
                                 if (players[i] != e && IsInsideWall(t.Value, playerT[i].Value))
                                 {
                                     PostUpdateCommands.DestroyEntity(e);
                                 }
                             }
                         });
        playerT.Dispose();
        players.Dispose();
    }
    
    private bool IsInsideWall(float3 pT, float3 p2T)
    {
        return math.abs(pT.x - p2T.x) < 0.9f && math.abs(pT.z - p2T.z) < 0.9f;
    }
}
