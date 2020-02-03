using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public enum ObstacleType
{
    Floor = 0,
    Stone = 1,
    Tree = 2
}

public class SceneManager : MonoBehaviour
{
    public bool IsInited = false;

    public ObstacleType[][] Map { get; private set; }

    public Mesh BlockMesh;
    public Mesh CapsuleMesh;

    public Material FloorMaterial;
    public Material UndergroundMaterial;
    public Material UndergroundMaterial2;
    public Material ObstacleMaterial;
    public Material TreeMaterial;

    public int Width = 100;
    public int Height = 100;
    public int Depth = 3;
    public float ObstacleRatio = 0.2f;
    public int PlayerCount = 10;
    public float Speed = 0.5f;

    public Material[] PlayerMaterials;

    private static SceneManager _instance;

    public static SceneManager GetInstance()
    {
        return _instance;
    }

    // Start is called before the first frame update
    void Start()
    {
        IsInited = false;
        _instance = this;

        Map = new ObstacleType[Height][];

        for (int i = 0; i < Height; i++)
        {
            Map[i] = new ObstacleType[Width];

            for (int j = 0; j < Width; j++)
            {
                Map[i][j] = ObstacleType.Floor;
            }
        }

        EntityManager entityManager = World.Active.EntityManager;

        EntityArchetype obstacleArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld));

        var floorRenderMesh = new RenderMesh()
        {
            mesh = BlockMesh,
            material = FloorMaterial
        };

        var undergroundRenderMesh = new RenderMesh()
        {
            mesh = BlockMesh,
            material = UndergroundMaterial
        };

        var undergroundRenderMesh2 = new RenderMesh()
        {
            mesh = BlockMesh,
            material = UndergroundMaterial2
        };

        var obstacleRenderMesh = new RenderMesh()
        {
            mesh = BlockMesh,
            material = ObstacleMaterial
        };

        var TreeRenderMesh = new RenderMesh()
        {
            mesh = BlockMesh,
            material = TreeMaterial
        };

        for (int i = 0; i < Height; i++)
        {
            for (int j = 0; j < Width; j++)
            {
                Map[i][j] = Random.Range(0f, 1f) > ObstacleRatio
                    ? ObstacleType.Floor
                    : (ObstacleType) Random.Range(1, 3);

                var depth = Map[i][j] == ObstacleType.Tree ? Random.Range(Depth + 1, Depth + 3) : Depth;

                for (int k = 0; k < depth; k++)
                {
                    RenderMesh renderMesh = floorRenderMesh;

                    switch (Map[i][j])
                    {
                        case ObstacleType.Floor:
                            if (k == 0)
                            {
                                renderMesh = undergroundRenderMesh2;
                            }
                            else if (k == 1)
                            {
                                renderMesh = undergroundRenderMesh;
                            }
                            else
                            {
                                renderMesh = floorRenderMesh;
                            }

                            break;
                        case ObstacleType.Stone:
                            if (k == 0)
                            {
                                renderMesh = undergroundRenderMesh;
                            }
                            else if (k == 1)
                            {
                                renderMesh = floorRenderMesh;
                            }
                            else
                            {
                                renderMesh = obstacleRenderMesh;
                            }

                            break;
                        case ObstacleType.Tree:
                            if (k == 0)
                            {
                                renderMesh = floorRenderMesh;
                            }
                            else if (k == 1)
                            {
                                renderMesh = obstacleRenderMesh;
                            }
                            else
                            {
                                renderMesh = TreeRenderMesh;
                            }

                            break;
                    }

                    var entity = entityManager.CreateEntity(obstacleArchetype);

                    entityManager.SetSharedComponentData(entity, renderMesh);
                    entityManager.SetComponentData(entity, new Translation
                    {
                        Value = new float3(i, k + (int) Map[i][j], j)
                    });
                }
            }
        }

        IsInited = true;
    }
}