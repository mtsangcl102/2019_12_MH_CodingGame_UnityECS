using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using Unity.Burst;

[UpdateBefore(typeof(DestroySystem))]
public class PhysicsSystem : ComponentSystem
{
    private EntityQuery collider_group;
    
    // Here we define the group 
    protected override void OnCreate()
    {
        // Get all colliders
        collider_group = GetEntityQuery(
            ComponentType.ReadOnly<Collider>(),
            typeof(Translation),
            ComponentType.ReadWrite<RigiBody>()
        );
    }
    
    [BurstCompile]
    protected override void OnUpdate()
    {
        var collidersTranslations = collider_group.ToComponentDataArray<Translation>(Allocator.TempJob);
        var colliders = collider_group.ToComponentDataArray<Collider>(Allocator.TempJob);
        var collidersEntities = collider_group.ToEntityArray(Allocator.TempJob);
        var collidersRigiBody = collider_group.ToComponentDataArray<RigiBody>(Allocator.TempJob);
        
        Entities.ForEach(( Entity entity, ref Translation translation, ref Collider collider, ref RigiBody rigidBody) => {
            if ( collider.bodyType == Collider.BodyType.Terrian ) return;
          
            for (int i = 0; i < colliders.Length; i++) {
                if ( entity == collidersEntities[i] ) continue;
                
                bool collied = AreSquaresOverlapping( translation, collider, ref rigidBody, collidersTranslations[i], colliders[i] );
                if (collied) {
                    rigidBody.collied = true;
                    rigidBody.Velocity = new float2(0,0);
                    if (colliders[i].bodyType == Collider.BodyType.Terrian &&
                        translation.Value.x == collidersTranslations[i].Value.x &&
                        translation.Value.z == collidersTranslations[i].Value.z &&
                        collider.bodyType != colliders[i].bodyType
                        ) {
                        rigidBody.colliedEnter = true;
                    }
                    if (collider.bodyType == colliders[i].bodyType) {
                        RigiBody otherRigiBody = collidersRigiBody[i];
                        otherRigiBody.colliedWithSoldier = true;
                        otherRigiBody.collied = true;
                        rigidBody.colliedWithSoldier = true;
                    }
                    break;
                }
            }
        });
        
        collidersTranslations.Dispose();
        colliders.Dispose();
        collidersEntities.Dispose();
        collidersRigiBody.Dispose();
    }
    
    static bool AreSquaresOverlapping(Translation translatioinA, Collider colliderA, ref RigiBody rigiBodyA, Translation translatioinB, Collider colliderB) {
        float sizeA = colliderA.Size;
        float sizeB = colliderB.Size;
        float3 posA = translatioinA.Value;
        float3 posB = translatioinB.Value;
        
        if (colliderA.bodyType != colliderB.bodyType) {
            float d = (sizeA / 2) + (sizeB / 2);
            return math.abs( translatioinA.Value.y - translatioinB.Value.y ) <= 0.5  && 
                   math.abs(posA.x + rigiBodyA.Velocity.x - posB.x) < d &&
                   math.abs(posA.z + rigiBodyA.Velocity.y - posB.z) < d;
        } else {
            float d = (sizeA / 2) + (sizeB / 2);
            return math.abs(posA.x + rigiBodyA.Velocity.x - posB.x) < d &&
                   math.abs(posA.z + rigiBodyA.Velocity.y - posB.z) < d;
        }
    }
}