using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Collections ;
using Unity.Rendering;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Unity.Jobs ;

public class ECSGameManager : MonoBehaviour
{
    [FormerlySerializedAs("mesh")] [SerializeField] private Mesh terrainMesh;
    [FormerlySerializedAs("materials")] [SerializeField] private Material[] terrainMaterials;
    [SerializeField] private Mesh playerMesh;
    [SerializeField] private Material playerMaterials;

    private EntityArchetype playerArchetype;

    private EntityManager entityManager;

    public static int[,] heightMap;
    public static bool[] treeMap;
    // Start is called before the first frame update
    private void Start()
    {
        treeMap = new bool[ 10000 ];
            
        entityManager = World.Active.EntityManager;
        EntityArchetype terrainArchetype = entityManager.CreateArchetype(
            typeof( Translation ),
            //typeof( WorldComponent ),
            typeof( RenderMesh ),
            typeof( LocalToWorld )
        );
        NativeArray<Entity> terrainArray = new NativeArray<Entity>(30000, Allocator.Temp);
        entityManager.CreateEntity( terrainArchetype, terrainArray );
        
        heightMap = new int[100,100];
        for (int i = 0; i < terrainArray.Length; i++)
        {
            Entity entity = terrainArray[i];
            if (i < 10000)
            {
                heightMap[i % 100, i / 100] = Mathf.Max(Random.Range(0, 34) - 30, 0);
                if (heightMap[i % 100, i / 100] > 0)
                {
                    treeMap[ i ] = true;
                }
            }

            int z = i / 10000 - heightMap[i % 100, (i / 100) % 100];
            entityManager.SetComponentData( entity, new Translation { Value = new float3( i % 100, ( i / 100 ) % 100, z ) } );
           // entityManager.SetSharedComponentData( entity, new WorldComponent(){ isPlayer = false }  );
            entityManager.SetSharedComponentData( entity, new RenderMesh { mesh = terrainMesh, material = terrainMaterials[ Mathf.Max(z * -1, 0 ) ] } );
        }
        
        
        playerArchetype = entityManager.CreateArchetype(
            typeof( Translation ),
            typeof( Rotation ),
            typeof( PlayerComponent ),
           // typeof( WorldComponent ),
            typeof( RenderMesh ),
            typeof( LocalToWorld )
        );

        terrainArray.Dispose();
    }

    private float _time = 0f;
    private void Update()
    {
        _time += Time.deltaTime;
        if (_time > 0.05f)
        {
            _time -= 0.05f;
            Entity entity = entityManager.CreateEntity( playerArchetype );

            int x = Random.Range(0, 100);
            int y = Random.Range(0, 100);
            if (heightMap[x, y] > 0)
            {
                entityManager.SetComponentData( entity, new Translation { Value = new float3( x, y, -1.5f ) } );
                entityManager.SetComponentData( entity, new Rotation { Value = Quaternion.Euler( 90f, 0f, 0f ) } );
                entityManager.SetComponentData( entity, new PlayerComponent(){ direction = Random.Range(0, 4) }  );
                //entityManager.SetSharedComponentData( entity, new WorldComponent(){ isPlayer = true }  );
                entityManager.SetSharedComponentData( entity, new RenderMesh { mesh = playerMesh, material = playerMaterials } );
            }
        }
    }
}

