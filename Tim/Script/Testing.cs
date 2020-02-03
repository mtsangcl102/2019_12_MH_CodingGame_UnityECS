using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Rendering;

public struct SpawnComponent : IComponentData
{
}

public struct ObstacleComponent : IComponentData
{
}


public class Testing : MonoBehaviour
{
    private const int HEIGHT_COUNT = 5;
    private static readonly Color[] BLOCK_COLOR = { Color.black, new Color(0.3f,0.3f,0.3f), Color.blue, new Color(0.7f,0.2f,0.2f), Color.green };
    [SerializeField] private Mesh mMesh;
    [SerializeField] private Material mMaterial;
    [SerializeField] private Material mCharacterMaterial;
    [SerializeField] private Mesh mCharacterMesh;

    public static Mesh CHARACTER_MESH;
    public static Material CHARACTER_MATERIAL;
    // Start is called before the first frame update
    void Start()
    {
        CHARACTER_MESH = mCharacterMesh;
        CHARACTER_MATERIAL = mCharacterMaterial;

        EntityManager entManager = World.Active.EntityManager;
        EntityArchetype entityArchetype = entManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld)
            );

        Material[] mArray = new Material[5];
        int i = 0;
        for (i = 0; i < 5; i++)
        {
            mArray[i] = Instantiate(mMaterial);
            mArray[i].color = BLOCK_COLOR[i];
        }
        NativeArray<Entity> entityArray = new NativeArray<Entity>(100*100*3, Allocator.Temp);
        entManager.CreateEntity(entityArchetype,entityArray);

        i = 0;
        int obsMaxCount = UnityEngine.Random.Range(400, 600);
        int currObsCount = obsMaxCount;

        for (int x=0; x <100; x++ )
        { 
            for( int y=0; y<100; y++ )
            {
                int height = 0;

                if (UnityEngine.Random.Range(0f,1f) <= (float)obsMaxCount/(100*100) )
                {
                    currObsCount--;
                    height = UnityEngine.Random.Range(1, 3);
                }

                for ( int t=0; t<3; t++,i++ )
                {
                    Entity entity = entityArray[i];

                    if( height > 0 && t==2 )
                        entManager.AddComponent(entity, typeof(ObstacleComponent));
                    
                    entManager.SetComponentData(entity, new Translation { Value = new float3(x, height+t, y) });
                    entManager.SetSharedComponentData(entity, new RenderMesh
                    {
                        mesh = mMesh,
                        material = mArray[height+t]
                    });



                }
            }
        }

        entityArray.Dispose();

    }

    // Update is called once per frame
    void Update()
    {

    }

}
