// using System;
// using Unity.Burst;
// using Unity.Collections;
// using Unity.Entities;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;
// using static Unity.Mathematics.math;
//
// [UpdateAfter(typeof(CharacterMoveSystem))]
// public class CollisionDetectionSystem : ComponentSystem
// {
//     private EntityQuery _obstacles;
//     private EntityQuery _characters;
//
//     protected override void OnCreate()
//     {
//         // check if it collide with obstacles
//         _obstacles = GetEntityQuery(ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<IsObstacleComponentData>());
//         _characters = GetEntityQuery(ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<MoveComponentData>());
//     }
//
//     protected override void OnUpdate()
//     {
//         var characterEntities = _characters.ToEntityArray( Allocator.TempJob );
//         var obstacleEntities = _obstacles.ToComponentDataArray<Translation>( Allocator.TempJob );
//
//         foreach ( var characterEntity in characterEntities )
//         {
//             var characterTranslation = EntityManager.GetComponentData<Translation>( characterEntity );
//             
//             var isCollided = false;
//             for ( var i = 0 ; i < obstacleEntities.Length ; ++ i )
//             {
//                 var obstacle = obstacleEntities[i];
//                 
//                 if ( isCollided )
//                     break;
//                 
//                 if ( Math.Abs( obstacle.Value.x - characterTranslation.Value.x ) < 1.0f &&
//                      Math.Abs( obstacle.Value.z - characterTranslation.Value.z ) < 1.0f )
//                 {
//                     _UpdatePositionAndMoveDirection( characterEntity , characterTranslation );
//                     isCollided = true;
//                 }
//             }
//             
//             // check boundary
//             if ( characterTranslation.Value.x < 0.5f ||
//                  characterTranslation.Value.x > 99.5f ||
//                  characterTranslation.Value.z < 0.5f ||
//                  characterTranslation.Value.z > 99.5f )
//             {
//                 _UpdatePositionAndMoveDirection( characterEntity , characterTranslation );
//             }
//         }
//         
//         
//         characterEntities.Dispose();
//         obstacleEntities.Dispose();
//     }
//
//     private void _UpdatePositionAndMoveDirection( Entity characterEntity , Translation characterTranslation )
//     {
//         // move direction
//         var moveComponentData = EntityManager.GetComponentData<MoveComponentData>( characterEntity );
//
//         // move back using move direction
//         var newPosition = characterTranslation.Value - moveComponentData.Value / 3f;
//         EntityManager.SetComponentData( characterEntity , new Translation() { Value = newPosition } );
//
//         // collided ! rotate it
//         moveComponentData.Value = new float3( moveComponentData.Value.z , 0 , -moveComponentData.Value.x );
//         EntityManager.SetComponentData( characterEntity , new MoveComponentData() { Value = moveComponentData.Value } );
//         
//         // Debug.Log( $"entity new position: {newPosition.ToString()}" );
//         // Debug.Log( $"entity new move: {moveComponentData.Value.ToString()}" );
//         // Debug.Log( $"entity collide with obstacle: {obstacle.Value.x}, {obstacle.Value.z}" );
//
//         // return moveComponentData;
//     }
// }