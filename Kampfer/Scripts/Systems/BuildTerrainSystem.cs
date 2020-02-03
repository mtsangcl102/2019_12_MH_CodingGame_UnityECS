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
    public class BuildTerrainSystem : ComponentSystem
    {
        private bool _mapLoad;

        protected override void OnUpdate()
        {
            if (_mapLoad || GameManager.GameConfig == null) return;
            
            _mapLoad = true;
            
            var rng = new Unity.Mathematics.Random();
            rng.InitState((uint)DateTime.UtcNow.Ticks);
            
            var entityManager = World.Active.EntityManager;
            var blockEntities = new NativeArray<Entity>(GameManager.GameConfig.TerrainSize * GameManager.GameConfig.TerrainSize * 3, Allocator.Temp);
            entityManager.CreateEntity(GameManager.BlockArchetype, blockEntities);
            
            var index = 0;
            
            var offsets = new float3((1f - GameManager.GameConfig.HighTreeRatio), (1f - GameManager.GameConfig.HighTreeRatio - GameManager.GameConfig.TreeRatio), (1f - GameManager.GameConfig.HighTreeRatio - GameManager.GameConfig.TreeRatio - GameManager.GameConfig.RockRatio));
            
            for (var x = 0; x < GameManager.GameConfig.TerrainSize; x++)
            {
                for (var y = 0; y < GameManager.GameConfig.TerrainSize; y++)
                {
                    var rndResult = rng.NextFloat();
                    var blockType = BlockType.LAND;
                    if (rndResult > offsets[0])
                    {
                        blockType = BlockType.HIGHTREE;
                    }
                    else if (rndResult > offsets[1])
                    {
                        blockType = BlockType.TREE;
                    }
                    else if (rndResult > offsets[2])
                    {
                        blockType = BlockType.ROCK;
                    }
                    
                    for (var level = 0; level < 3; level++)
                    {
                        _CreateBlock(entityManager, blockEntities[index], blockType, level, new int2(x, y));
                        index++;
                    }
                }
            }
            
            blockEntities.Dispose();
        }
        
        private static void _CreateBlock(EntityManager entityManager, Entity cubeEntity, BlockType blockType, int level, int2 xy)
        {
            var matIndex = level + (int) blockType;
            
            entityManager.SetSharedComponentData(cubeEntity, new RenderMesh { mesh = GameManager.GameConfig.BlockMesh, material = GameManager.GameConfig.BlockMaterials[matIndex], receiveShadows = matIndex > 1, castShadows = matIndex > 2 ? ShadowCastingMode.On : ShadowCastingMode.Off});
            entityManager.SetComponentData(cubeEntity, new Translation { Value = new float3(xy.x, (int) blockType + level, xy.y) });
            entityManager.SetComponentData(cubeEntity, new BlockComponent { BlockType = blockType, X = xy.x, Y = xy.y, Z = (int) blockType} );

            switch (blockType)
            {
                case BlockType.ROCK:
                    if(level == 2) entityManager.AddComponentData(cubeEntity, new ObstacleComponent{});
                    break;
                case BlockType.TREE:
                    if(level == 1) entityManager.AddComponentData(cubeEntity, new ObstacleComponent{});
                    break;
                case BlockType.HIGHTREE:
                    if(level == 0) entityManager.AddComponentData(cubeEntity, new ObstacleComponent{});
                    break;
            }
        }
    }
}