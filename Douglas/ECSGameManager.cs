using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Jobs;

[BurstCompile(CompileSynchronously = true)]
public class ECSGameManager : MonoBehaviour {
    [SerializeField] private Mesh _terrainMesh;
    [SerializeField] private Material[] _materials;
    [SerializeField] private Mesh _characterMesh;
    [SerializeField] private Material characterMaterial;
    [SerializeField] private int count;
    [SerializeField] private bool getEntityCharacterQuery;
    [SerializeField] private int characterCurrentCount = 0;
    EntityArchetype entityCharacterArchetype;

    private List<Translation> translations;
    private float time = 0f;
    private JobHandle moveJobHandle;
    EntityManager entityManager;

    const int TERRAIN_SIZE_LIMIT = 100 * 100 * 3;

    void Start() {
        entityManager = World.Active.EntityManager;
        EntityArchetype entityTerrainArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );


        NativeArray<Entity> entityTerrainArray = new NativeArray<Entity>(TERRAIN_SIZE_LIMIT, Allocator.Temp);
        entityManager.CreateEntity(entityTerrainArchetype, entityTerrainArray);
        count = 0;
        for(int i = 0; i < entityTerrainArray.Length; i++) {
            int y = math.max(0, UnityEngine.Random.Range(-35, 3));
            for(int j = 0; j < 3; j++) {
                if(j == 2 && y != 0) {
                    Entity entity = entityTerrainArray[i];
                    entityManager.SetName(entity, "terrain_" + y + "_" + (y + j));
                    entityManager.SetComponentData(entity, new Translation { Value = new float3(count % 100, y + j, count / 100) });
                    //entityManager.AddComponentObject(entity, AABBComponent);
                    entityManager.AddComponentData<AABBComponent>(entity, new AABBComponent {
                        bound = 0.5f,
                        status = (j == 2) ? 1 : 0
                    });
                    entityManager.AddComponentData<MoveComponent>(entity, new MoveComponent {
                        moveSpeed = 0,
                        direction = (MoveComponent.DIRECTION) 0,
                        isSuddenChange = false,
                        isObstacle = true,
                    });
                    entityManager.SetSharedComponentData(entity, new RenderMesh {
                        mesh = _terrainMesh,
                        material = _materials[y + j],
                    });
                } else {
                    Entity entity = entityTerrainArray[i];
                    entityManager.SetName(entity, "terrain_" + y + "_" + (y + j));
                    entityManager.SetComponentData(entity, new Translation { Value = new float3(count % 100, y + j, count / 100) });
                    //entityManager.SetComponentData(entity, new AABBComponent {
                    //    min = new float3(count % 100, y + j, count / 100) + 0.5f,
                    //    max = new float3(count % 100, y + j, count / 100) - 0.5f,
                    //    status = (j == 2) ? 1 : 0
                    //});
                    entityManager.SetSharedComponentData(entity, new RenderMesh {
                        mesh = _terrainMesh,
                        material = _materials[y + j],
                    });
                }
                i++;
            }
            count++;
            i--;
        }

        entityTerrainArray.Dispose();
        entityCharacterArchetype = entityManager.CreateArchetype(
            typeof(AABBComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(MoveComponent)
        );

    }

    // Update is called once per frame
    void Update() {
        time += Time.deltaTime;
        if(time > 0.5f || characterCurrentCount < 10) {
            //entityCharacterArray = new NativeArray<Entity>(characterSizeLimit, Allocator.Temp);
            Entity entity = entityManager.CreateEntity(entityCharacterArchetype);

            float3 tempFloat3 = new float3(UnityEngine.Random.Range(20f, 80f), 3.5f, UnityEngine.Random.Range(20f, 80f));
            //float3 tempFloat3 = new float3(50f, 3f, 50f);
            entityManager.SetName(entity, "character" + characterCurrentCount);
            entityManager.SetComponentData(entity, new AABBComponent {
                bound = 0.3f,
                status = 2
            });
            entityManager.SetComponentData(entity, new MoveComponent {
                moveSpeed = UnityEngine.Random.Range(5f, 10f),
                direction = (MoveComponent.DIRECTION) UnityEngine.Random.Range(0, 4),
                isSuddenChange = false,
                isObstacle = false,

            });
            entityManager.SetComponentData(entity, new Translation { Value = tempFloat3 });

            entityManager.SetSharedComponentData(entity, new RenderMesh {
                mesh = _characterMesh,
                material = characterMaterial,
            });
            characterCurrentCount++;
            time = 0f;
        }
    }
}