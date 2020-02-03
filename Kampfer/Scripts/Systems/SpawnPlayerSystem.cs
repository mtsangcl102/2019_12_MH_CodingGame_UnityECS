using System;
using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering;

namespace CodingGame
{
    public class SpawnPlayerSystem : ComponentSystem {

        private static Unity.Mathematics.Random _rng;
        protected override void OnCreate()
        {
            _rng = new Unity.Mathematics.Random();
            base.OnCreate();
        }
        
        protected override void OnUpdate() {
            _rng.InitState((uint)DateTime.UtcNow.Ticks);
            
            var entityManager = World.Active.EntityManager;
            var query = entityManager.CreateEntityQuery(typeof(PlayerComponent));
            var spawnCount = GameManager.GameConfig.PlayerCount - query.CalculateEntityCount();
            
            for (var i = 0; i < spawnCount; i++)
            {
                _SpawnPlayer(entityManager);
            }
        }
        
        private static void _SpawnPlayer(EntityManager entityManager)
        {
            var playerEntity = entityManager.CreateEntity(GameManager.PlayerArchetype);
            var spawnPosition = _rng.NextInt2(new int2(0,0), new int2(GameManager.GameConfig.TerrainSize - 1, GameManager.GameConfig.TerrainSize - 1));
            var materialIndex = _rng.NextInt(0, GameManager.GameConfig.PlayerMaterials.Length);
            var material = GameManager.GameConfig.PlayerMaterials[materialIndex];

            entityManager.SetSharedComponentData(playerEntity, new RenderMesh { mesh = GameManager.GameConfig.PlayerMesh, material = material, receiveShadows = false, castShadows = ShadowCastingMode.On});
            entityManager.SetComponentData(playerEntity, new PlayerComponent { MaterialIndex = materialIndex});
            entityManager.SetComponentData(playerEntity, new Translation() { Value = new float3(spawnPosition.x, 3.5f, spawnPosition.y)});
            entityManager.SetComponentData(playerEntity, new MoveSpeedComponent{Value = _rng.NextFloat(GameManager.GameConfig.PlayerSpeedMin, GameManager.GameConfig.PlayerSpeedMax)});
            entityManager.SetComponentData(playerEntity, new InitComponent{X = spawnPosition.x, Y = spawnPosition.y});

        }
    }
}