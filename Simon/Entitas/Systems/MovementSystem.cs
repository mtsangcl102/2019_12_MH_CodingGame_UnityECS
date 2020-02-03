using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

public class MovementSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref PlayerComponent playerComponent, ref Translation translation ) =>
        {
            float angle = Mathf.Atan2(playerComponent.Direction.y, playerComponent.Direction.x);
            angle += Random.Range(-Mathf.PI / 4 * Time.deltaTime, Mathf.PI / 4 * Time.deltaTime);
            playerComponent.Direction = new Vector2( Mathf.Cos(angle), Mathf.Sin(angle) );
            
            translation.Value += new float3( 
                playerComponent.Speed * Time.deltaTime * playerComponent.Direction.x ,
                0,
                playerComponent.Speed * Time.deltaTime * playerComponent.Direction.y );

            if (translation.Value.x < 0 && playerComponent.Direction.x < 0 ||
                translation.Value.x > 100 && playerComponent.Direction.x > 0)
            {
                playerComponent.Direction.x = -playerComponent.Direction.x;
            }
            
            if (translation.Value.z < 0 && playerComponent.Direction.y < 0 ||
                translation.Value.z > 100 && playerComponent.Direction.y > 0)
            {
                playerComponent.Direction.y = -playerComponent.Direction.y;
            }
        });
    }
}
