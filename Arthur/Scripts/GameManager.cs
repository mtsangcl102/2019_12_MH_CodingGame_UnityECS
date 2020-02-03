using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Jobs;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static readonly int mapSize = 100;
    public static float spawnRate;
    
    
    [SerializeField] private Mesh mapTileMesh, charMesh;
    [SerializeField] private Material[] mapTileMaterials;
    [SerializeField] private Material charMaterial;
    [SerializeField] private float obstacleChance, normalHeight, obstacleHeight, randomHeightRange;

    [SerializeField] private float _spawnRate;
    
    private EntityManager manager;

    void MapGen()
    {
        EntityArchetype tileArcheType = manager.CreateArchetype(
            typeof(RenderMesh), 
            typeof(LocalToWorld), 
            typeof(Translation), 
            typeof(Rotation), 
            typeof(NonUniformScale));
        
        EntityArchetype mapArcheType = manager.CreateArchetype(
            typeof(Dat_MapTileData));
        
        NativeArray<Entity> mapTiles = new NativeArray<Entity>(mapSize * mapSize * 3, Allocator.Temp);
        manager.CreateEntity(tileArcheType, mapTiles);
        
        NativeArray<Entity> map = new NativeArray<Entity>(mapSize * mapSize, Allocator.Temp);
        manager.CreateEntity(mapArcheType, map);
        
        for (int i = 0; i < mapSize * mapSize * 3; i += 3)
        {
            bool isObstacle = Random.value < obstacleChance;
            int tileLv = isObstacle ? Random.Range(1, 4) : 0;
            
            for (int j = 0; j < 3; j++)
            {
                var tile = mapTiles[i + j];
                
                manager.SetSharedComponentData(tile, new RenderMesh{mesh = mapTileMesh, material = mapTileMaterials[tileLv + j]});
                manager.SetComponentData(tile, new Translation{Value = new float3(i / 3 % mapSize, -2.5f + tileLv + j, i / mapSize / 3)});
                manager.SetComponentData(tile, new Rotation{Value = quaternion.identity});
                manager.SetComponentData(tile, new NonUniformScale{Value = new float3(1f, 1f, 1f)});
            }
            
            var m = map[i / 3];
            manager.SetComponentData(m, new Dat_MapTileData
            {
                id = i / 3,
                isObstacle = isObstacle
            });
            
/*            var tileBot = mapTiles[i];
            var tileMid = mapTiles[i + 1];
            var tileTop = mapTiles[i + 2];
            
            bool isObstacle = UnityEngine.Random.value < obstacleChance;
            int tileLv = isObstacle ? Random.Range(1, 4) : 0;
            
            manager.SetSharedComponentData(tileBot, new RenderMesh{mesh = mapTileMesh, material = mapTileMaterials[tileLv]});
            manager.SetSharedComponentData(tileMid, new RenderMesh{mesh = mapTileMesh, material = mapTileMaterials[tileLv + 1]});
            manager.SetSharedComponentData(tileTop, new RenderMesh{mesh = mapTileMesh, material = mapTileMaterials[tileLv + 2]});
            
            manager.SetComponentData(tileBot, new Translation{Value = new float3(i / 3 % mapSize, -2.5f + tileLv, (i) / mapSize / 3)});
            manager.SetComponentData(tileMid, new Translation{Value = new float3(i / 3 % mapSize, -2.5f + tileLv + 1f, (i) / mapSize / 3 )});
            manager.SetComponentData(tileTop, new Translation{Value = new float3(i / 3 % mapSize, -2.5f + tileLv + 2f, (i) / mapSize  / 3)});
            
            manager.SetComponentData(tileBot, new Rotation{Value = quaternion.identity});
            manager.SetComponentData(tileMid, new Rotation{Value = quaternion.identity});
            manager.SetComponentData(tileTop, new Rotation{Value = quaternion.identity});
            
            manager.SetComponentData(tileBot, new NonUniformScale{Value = new float3(1f, 1f, 1f)});
            manager.SetComponentData(tileMid, new NonUniformScale{Value = new float3(1f, 1f, 1f)});
            manager.SetComponentData(tileTop, new NonUniformScale{Value = new float3(1f, 1f, 1f)});
            
            manager.SetComponentData(tileBot, new Dat_MapTileData{isObstacle = isObstacle});
            manager.SetComponentData(tileMid, new Dat_MapTileData{isObstacle = isObstacle});
            manager.SetComponentData(tileTop, new Dat_MapTileData{isObstacle = isObstacle});*/
        }
        
        mapTiles.Dispose();
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        spawnRate = _spawnRate;
        
        manager = World.Active.EntityManager;
        MapGen();
    }

    // Update is called once per frame
    void Update()
    {
        EntityArchetype tileArcheType = manager.CreateArchetype(
            typeof(RenderMesh), 
            typeof(LocalToWorld), 
            typeof(Translation), 
            typeof(Rotation), 
            typeof(NonUniformScale), 
            typeof(Dat_MovementData));
        
        Entity c = manager.CreateEntity(tileArcheType);
        bool isVertical = Random.value > 0.5f;
        
        manager.SetSharedComponentData(c, new RenderMesh{mesh = charMesh, material = charMaterial});
        manager.SetComponentData(c, new Translation{Value = new float3(Random.Range(0, 100), 1f, Random.Range(0, 100))});
        manager.SetComponentData(c, new Rotation{Value = quaternion.identity});
        manager.SetComponentData(c, new NonUniformScale{Value = new float3(1f, 1f, 1f)});
        manager.SetComponentData(c, new Dat_MovementData
        {
            dir = new float2(isVertical ? 0f : Random.Range(-1f, 1f) > 0f ? 1f : -1f, isVertical ? Random.Range(-1f, 1f) > 0f ? 1f : -1f : 0f),
            speed = 1f
        });
    }
}
