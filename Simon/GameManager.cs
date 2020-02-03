using System.Collections;
using System.Collections.Generic;
using TreeEditor;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;
using Unity.Rendering;
using UnityEngine.Experimental.U2D;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    [SerializeField] public Mesh TerrainMesh;
    [SerializeField] public Mesh PlayerMesh;
    [SerializeField] public Material[] GroundMaterials;
    [SerializeField] public Material PlayerMaterial;

    private const float CreatePlayerInterval = 2;
    private float _createPlayerTimer;
    
    // Start is called before the first frame update
    void Start()
    {
        _CreateTerrains();
        _CreatePlayers(100);
        _createPlayerTimer = 0;
    }

    void Update()
    {
        _createPlayerTimer += Time.deltaTime;
        if (_createPlayerTimer >= CreatePlayerInterval)
        {
            _createPlayerTimer -= CreatePlayerInterval;
            _CreatePlayers(1);
        }
    }

    private void _CreateTerrains()
    {
        EntityManager entityManager = World.Active.EntityManager;
        EntityArchetype obsticleType = entityManager.CreateArchetype(
            typeof(TerrainComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );
        EntityArchetype groundType = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );
        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {

                Enum.TerrainType type = Enum.TerrainType.None;
                float height = 1;

                var random = Random.Range(0, 100);
                if (random > 98)
                {
                    type = Enum.TerrainType.Tree;
                    height = Random.Range(3f, 4f);
                }
                else if (random > 96)
                {
                    type = Enum.TerrainType.Rock;
                    height = Random.Range(2f, 3f);
                }

                for (int i = 0; i < 3; i++)
                {
                    var thisType = i > 0 ? Enum.TerrainType.None : type;
                    Entity entity = entityManager.CreateEntity(thisType == Enum.TerrainType.None ? groundType : obsticleType);
                    entityManager.SetComponentData(entity, new Translation{ Value = new float3( x, -4.5f + height + i, y)});
                    Material material;
                    if (type == Enum.TerrainType.Rock)
                    {
                        material = GroundMaterials[3];
                    }
                    else if (type == Enum.TerrainType.Tree)
                    {
                        material = GroundMaterials[4];
                    }
                    else
                    {
                        material = GroundMaterials[Mathf.FloorToInt(height + i - 1)];
                    }
                    
                    entityManager.SetSharedComponentData(entity, new RenderMesh{ mesh = TerrainMesh, material = material
                    });
                    if (thisType != Enum.TerrainType.None)
                    {
                        entityManager.SetComponentData(entity, new TerrainComponent
                        {
                            Type = type,
                            Height = height,
                        });
                    }
                }
            }
        }
    }

    private void _CreatePlayers(int count)
    {
        EntityManager entityManager = World.Active.EntityManager;
        EntityArchetype terrainType = entityManager.CreateArchetype(
            typeof(PlayerComponent),
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
        );
        for (int i = 0; i < count; i++)
        {
            Entity entity = entityManager.CreateEntity(terrainType);

            var position = new float3(Random.Range(0, 100f), 0, Random.Range(0, 100f));
            var angle = Random.Range(0, 360f);
            var direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            var speed = Random.Range(4, 6);
            entityManager.SetComponentData(entity, new PlayerComponent{ Direction = direction, Speed = speed , CheckTime = 0});
            entityManager.SetComponentData(entity, new Translation{ Value = position });
            entityManager.SetSharedComponentData(entity, new RenderMesh{ mesh = PlayerMesh, material = PlayerMaterial
            });
        }
    }
}
