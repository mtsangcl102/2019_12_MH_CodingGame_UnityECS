using System;
using Unity.Collections;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

namespace CodingGame
{
    public class GameManager : MonoBehaviour
    {
        public static EntityArchetype BlockArchetype;
        public static EntityArchetype PlayerArchetype;
        public static EntityArchetype ExplosionArchetype;
        public static GameConfig GameConfig;


        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var entityManager = World.Active.EntityManager;

            BlockArchetype = entityManager.CreateArchetype(
                typeof(Terrain),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(BlockComponent)
            );
            PlayerArchetype = entityManager.CreateArchetype(
                typeof(PlayerComponent),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(MoveSpeedComponent),
                typeof(InitComponent)
            );
            
            ExplosionArchetype = entityManager.CreateArchetype(
                typeof(ExplosionComponent),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(Translation),
                typeof(Scale),
                typeof(VelocityComponent)
            );
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void InitializeWithScene()
        {
            var gameConfigGameObject = GameObject.Find("GameManager");
            GameConfig = gameConfigGameObject == null ? null : gameConfigGameObject.GetComponent<GameConfig>();
            if (GameConfig == null) return;
        }
    }
}