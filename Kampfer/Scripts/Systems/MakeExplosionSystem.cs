using System;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Rendering;

namespace CodingGame
{
    public class MakeExplosionSystem : ComponentSystem {

        private static Unity.Mathematics.Random _rng;
        protected override void OnCreate()
        {
            _rng = new Unity.Mathematics.Random();
            base.OnCreate();
        }
        
        protected override void OnUpdate() {
            _rng.InitState((uint)DateTime.UtcNow.Ticks);
            
            var entityManager = World.Active.EntityManager;
            var query = entityManager.CreateEntityQuery(typeof(ExplosionCenterComponent));

            if (GameManager.GameConfig.CanExplosion)
            {
                var explosionCenterComponents = query.ToComponentDataArray<ExplosionCenterComponent>(Allocator.TempJob);
                var spawnCount = GameManager.GameConfig.ParticleCount;

                for (var i = 0; i < explosionCenterComponents.Length; i++)
                {
                    for (var j = 0; j < spawnCount; j++)
                    {
                        _CreateExplosion(entityManager, explosionCenterComponents[i]);
                    }
                }
                
                explosionCenterComponents.Dispose();
            }

            var entityArray = query.ToEntityArray( Allocator.TempJob );
            entityManager.DestroyEntity(entityArray);
            
            entityArray.Dispose();
            query.Dispose();
        }
        
        private static void _CreateExplosion(EntityManager entityManager, ExplosionCenterComponent explosionCenterComponent)
        {
            var playerEntity = entityManager.CreateEntity(GameManager.ExplosionArchetype);
            var velocity = _rng.NextFloat3Direction() * _rng.NextFloat(GameManager.GameConfig.ParticleVelocityMin, GameManager.GameConfig.ParticleVelocityMax) + math.float3(0f, 30f, 0f);
            var size = _rng.NextFloat(GameManager.GameConfig.ParticleSizeMin, GameManager.GameConfig.ParticleSizeMax);
            var material = GameManager.GameConfig.PlayerMaterials[explosionCenterComponent.MaterialIndex];

            entityManager.SetSharedComponentData(playerEntity, new RenderMesh { mesh = GameManager.GameConfig.ParticleMesh, material = material, receiveShadows = false, castShadows = ShadowCastingMode.On});
            entityManager.SetComponentData(playerEntity, new Translation() { Value = explosionCenterComponent.Position});
            entityManager.SetComponentData(playerEntity, new Scale() { Value = size});
            entityManager.SetComponentData(playerEntity, new VelocityComponent(){Value = velocity});

        }
    }
}