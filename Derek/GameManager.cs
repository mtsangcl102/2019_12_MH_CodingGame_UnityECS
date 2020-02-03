using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using Unity.Physics.Authoring;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{

    [FormerlySerializedAs("mesh")] [SerializeField] private UnityEngine.Mesh terrainMesh;
    [SerializeField] private UnityEngine.Mesh playerMesh;
    [SerializeField] private UnityEngine.Material[] playerMat;
    [SerializeField] private UnityEngine.Material[] terrainMat;

    private EntityManager entityManager;

    public static NativeArray<bool> wallMap; 

    private int numOfDingo = 0;
    public static List<Entity> destroyEntity = new List<Entity>();
    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.Active.EntityManager;
        wallMap = new NativeArray<bool>(101 * 101, Allocator.Persistent);

        EntityArchetype cubeArchetype = entityManager.CreateArchetype(
            typeof(Translation), 
            typeof(RenderMesh),
            typeof(LocalToWorld));

        for (int i = 0; i < 100; i++)
        {
            for (int ii = 0; ii < 100; ii++)
            {
                // determine init pos (y randomly set 1/2/3)
                float targetY = 0f;
                float randValue = Random.Range(0f, 1f);
                if (randValue < 0.05f)
                {
                    targetY = 2f;
                }
                else if (randValue < 0.1f)
                {
                    targetY = 1f;
                }
                
                float3 bottomPos = new float3(i, targetY, ii);
                
                NativeArray<Entity> entityArray = new NativeArray<Entity>(3, Allocator.Temp);
                entityManager.CreateEntity(cubeArchetype, entityArray);
                
                for (int iii = 0; iii < 3; iii++)
                {
                    entityManager.SetComponentData(entityArray[iii], 
                        new Translation
                        {
                            Value = bottomPos + new float3(0, iii, 0)
                        });
                    
                    entityManager.SetSharedComponentData(entityArray[iii], new RenderMesh
                    {
                        mesh = terrainMesh,
                        material = terrainMat[(int)entityManager.GetComponentData<Translation>(entityArray[iii]).Value.y],
                        subMesh = 0,
                        castShadows = ShadowCastingMode.On,
                    });

                    // above floor
                    if (targetY > 0f)
                    {
                        wallMap[i * 100 + ii] = true;
                        entityManager.AddComponentData(entityArray[iii], new ColliderComponent()
                        {
                            size = 0.5f
                        });
                        
                        entityManager.AddComponentData(entityArray[iii], new DingoDangerComponent()
                        {
                            
                        });
                        numOfDingo++;
                    }
                }
                entityArray.Dispose();
            }
        }
    }

    private float time = 0f;
    void Update()
    {
        time += Time.deltaTime;
        if ((int) time % 1 == 0)
        {
            time = 0;

            NativeArray<Entity> playersToAdd = new NativeArray<Entity>(100, Allocator.Temp);

            EntityArchetype playerArchetype = entityManager.CreateArchetype(
                typeof(PlayerComponent),
                typeof(Translation),
                typeof(RenderMesh),
                typeof(LocalToWorld),
                typeof(MoveSpeedComponent),
                typeof(ColliderComponent));

            // entityManager.CreateEntity(playerArchetype, playersToAdd);

            float x = Random.Range(0, 100);
            float y = 3.5f;
            float z = Random.Range(0, 100);

            if (wallMap[(int) x * 100 + (int) z]) return;

            Entity p = entityManager.CreateEntity(playerArchetype);
            entityManager.SetComponentData(p, new Translation()
            {
                Value = new float3(x, y, z),
            });

            entityManager.SetSharedComponentData(p, new RenderMesh()
            {
                mesh = playerMesh,
                material = playerMat[Random.Range(0, 3)],
            });

            entityManager.SetComponentData(p, new MoveSpeedComponent()
            {
                direction = Random.Range(0, 4)
            });

            entityManager.SetComponentData(p, new ColliderComponent()
            {
                size = 0.5f
            });

            // playersToAdd.Dispose();
        }
    }

    private void OnDestroy()
    {
        wallMap.Dispose();
    }
}

