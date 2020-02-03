using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Rendering;
using Random = UnityEngine.Random;
using Unity.Burst;
using UnityEngine.Rendering;

public class GameManager : MonoBehaviour {
    [SerializeField] private Mesh _terrianMesh;
    [SerializeField] private Material[] _terrianMaterialList;
    [SerializeField] private Mesh _soldierMesh;
    private EntityManager _entityManager;
    public const int terrianAmount = 100 * 100 * 3;
    
    // Start is called before the first frame update
    void Start() {
        _entityManager = World.Active.EntityManager;
        CreateTerrian();
    }

    void Update() {
        if ( Time.deltaTime * 20 > 0.2 )
            CreateSoldier();
    }

    [BurstCompile]
    void CreateTerrian() {
        EntityArchetype tempEntity = _entityManager.CreateArchetype(
            typeof(RenderMesh),
            typeof(Translation),
            typeof(LocalToWorld),
            typeof(Collider)
        );

        NativeArray<Entity> entitesList = new NativeArray<Entity>(terrianAmount, Allocator.Temp);
        _entityManager.CreateEntity(tempEntity, entitesList);

        int tempHight = 0;
        //layer
        for (int i = 0; i < 100; i++) {
            for (int j = 0; j < 100; j++) {
                tempHight = Random.Range(0, 100) > 95 ? Random.Range(1, 4) : Random.Range(0, 100) > 96 ? Random.Range(0, 2) : 0;

                for (int k = 0; k < 3; k++) {
                    Entity tempCube = entitesList[k + 3 * j + 300 * i];
                    Material tmpMaterial = _terrianMaterialList[tempHight + k];

                    _entityManager.SetSharedComponentData(tempCube, new RenderMesh {
                        mesh = _terrianMesh,
                        material = tmpMaterial,
                        castShadows = ShadowCastingMode.On,
                        receiveShadows = true
                    });
                    _entityManager.SetComponentData(tempCube, new Translation {
                        Value = new float3(i, k + tempHight, j)
                    });
                    _entityManager.SetComponentData(tempCube, new Collider {
                        Size = 1f,
                        bodyType = Collider.BodyType.Terrian
                    });

                    if (k + tempHight == 3) {
                        _entityManager.AddComponent(tempCube, typeof(RigiBody));
                        _entityManager.SetComponentData(tempCube, new RigiBody {
                            Velocity = new float2(0,0),
                            collied = false,
                            colliedEnter = false,
                            colliedWithSoldier = false
                        });
                    }
                }
            }
        }
    }

    void CreateSoldier() {
        EntityArchetype tempEntity = _entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(Collider),
            typeof(RigiBody)
        );

        Entity entity = _entityManager.CreateEntity(tempEntity);
        _entityManager.SetSharedComponentData(entity, new RenderMesh {
            mesh = _soldierMesh,
            material = _terrianMaterialList[Random.Range(0, _terrianMaterialList.Length - 1)],
            castShadows = ShadowCastingMode.On,
            receiveShadows = true
        });

        _entityManager.SetComponentData(entity, new Translation {
            Value = new float3(Random.Range(5, 95), 3.5f, Random.Range(5, 95))
        });

        _entityManager.SetComponentData(entity, new RigiBody() {
            Velocity = RandomVelocity(),
            collied = false,
            colliedEnter = false,
            colliedWithSoldier = false
        });
        
        _entityManager.SetComponentData(entity, new Collider {
            Size = 1f,
            bodyType = Collider.BodyType.Soldier
        });
    }
    
    
    public static float2 RandomVelocity() {
        var rand = new System.Random();
        int x = rand.Next(-1, 2);
        int y = 0;
        if (x == 0) {
            while ( y == 0 ) {
                y = rand.Next(-1, 2);
            }
        }
        return new float2(x,y);
    }
}

