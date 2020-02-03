using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Burst;

[UpdateBefore(typeof(SoldierSystem))]
public class DestroySystem : ComponentSystem
{
   [BurstCompile]
   protected override void OnUpdate() {
      Entities.ForEach( (Entity entity, ref Collider collider, ref RigiBody rigidbody) =>
      {
         if ( collider.bodyType == Collider.BodyType.Terrian ) return;
         
         if (rigidbody.colliedWithSoldier || rigidbody.colliedEnter) {
            PostUpdateCommands.DestroyEntity(entity);
         }
      });
   }
}
