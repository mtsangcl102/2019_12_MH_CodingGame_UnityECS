using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Random = UnityEngine.Random;

public class CollisionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity player1, ref PlayerComponent playerComponent1, ref Translation translation1) =>
        {
            if (playerComponent1.CheckTime > 0)
            {
                playerComponent1.CheckTime -= Time.deltaTime;
            }
            else
            {
                playerComponent1.CheckTime = Random.Range(0.1f,0.2f);
                
                var position1 = translation1.Value;
                // collide with player
                Entities.ForEach((Entity player2, ref PlayerComponent playerComponent2, ref Translation translation2) =>
                {
                    if (player1 != player2)
                    {
                        if (math.distance(position1, translation2.Value) < 1)
                        {
                            PostUpdateCommands.DestroyEntity(player1);
                            PostUpdateCommands.DestroyEntity(player2);
                        }
                    }
                });

                // collide with obsticle

                Vector2 direction = playerComponent1.Direction;
                bool doReflectX = false;
                bool doReflectY = false;
                Entities.ForEach(
                    (Entity player2, ref TerrainComponent terrainComponent, ref Translation translation2) =>
                    {
                        var obsticlePosition = new float3(translation2.Value.x, 0, translation2.Value.z);
                        var d = math.distance(position1, obsticlePosition);
                        if (d < 0.3f)
                        {
                            PostUpdateCommands.DestroyEntity(player1);
                        }
                        else if (d < 2)
                        {
                            if (Mathf.Abs(position1.x - obsticlePosition.x) < 1.2f)
                                if ((position1.x > obsticlePosition.x && direction.x < 0) ||
                                    (position1.x < obsticlePosition.x && direction.x > 0))
                                {
                                    doReflectX = true;
                                }

                            if (Mathf.Abs(position1.z - obsticlePosition.z) < 1.2f)
                                if ((position1.z > obsticlePosition.z && direction.y < 0) ||
                                    (position1.z < obsticlePosition.z && direction.y > 0))
                                {
                                    doReflectY = true;
                                }
                        }
                    });

                if (doReflectX)
                {
                    playerComponent1.Direction.x = -playerComponent1.Direction.x;
                }

                if (doReflectY)
                {
                    playerComponent1.Direction.y = -playerComponent1.Direction.y;
                }
            }
        });
    }
}
