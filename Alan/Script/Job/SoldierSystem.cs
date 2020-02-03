using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateAfter(typeof(DestroySystem))]
public class SoldierSystem : JobComponentSystem {
    struct SoldierMovementJob : IJobForEach<LocalToWorld, Translation, Collider, RigiBody> {
        public void Execute(ref LocalToWorld localToWorld, ref Translation translation, ref Collider collider, ref RigiBody rigiBody) {
            if ( collider.bodyType == Collider.BodyType.Terrian ) return;

            var rand = new System.Random();

            if (!rigiBody.collied) {
                if (rand.Next(0, 100) > 95) {
                    rigiBody.Velocity = GameManager.RandomVelocity();
                    return;
                }
                
                if (translation.Value.x + rigiBody.Velocity.x >= 99 ||
                    translation.Value.z + rigiBody.Velocity.y >= 99 || 
                    translation.Value.x + rigiBody.Velocity.x <= 0 ||
                    translation.Value.z + rigiBody.Velocity.y <= 0 ) {
                    rigiBody.Velocity = GameManager.RandomVelocity();
                    return;
                }
                
                translation.Value = new float3(
                    translation.Value.x + rigiBody.Velocity.x ,
                    translation.Value.y + 0f,
                    translation.Value.z + rigiBody.Velocity.y);
            }
            else {
                float2 recordFloat = new float2(rigiBody.Velocity.x, rigiBody.Velocity.y);
                while (rigiBody.Velocity.x == recordFloat.x && rigiBody.Velocity.y == recordFloat.y ) {
                    rigiBody.Velocity = GameManager.RandomVelocity();
                }
                rigiBody.collied = false;
            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new SoldierMovementJob() { };
        return job.Schedule(this, inputDependencies);
    }
}


