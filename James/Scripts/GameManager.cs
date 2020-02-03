using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

public class GameManager : MonoBehaviour
{
    public Mesh CubeMesh;
    public Mesh CapsuleMesh;
    public Material material;

    void Start()
    {       
        EntityManager entityManager = World.Active.EntityManager;

        EntityArchetype entityArchetype = entityManager.CreateArchetype( typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld) );

        NativeArray<Entity> entityArray = new NativeArray<Entity>( 30000,  Allocator.Temp );


        entityManager.CreateEntity( entityArchetype, entityArray );

        Material[] materials = new Material[]{
            GameObject.Instantiate( material ),
            GameObject.Instantiate( material ),
            GameObject.Instantiate( material ),
            GameObject.Instantiate( material ),
            GameObject.Instantiate( material )
        };
        materials[ 0 ].color = new Color( 1f, 0f, 0f, 1f );
        materials[ 1 ].color = new Color( 0f, 1f, 0f, 1f );
        materials[ 2 ].color = new Color( 0f, 0f, 1f, 1f );
        materials[ 3 ].color = new Color( 1f, 1f, 0f, 1f );
        materials[ 4 ].color = new Color( 0f, 1f, 1f, 1f );

		ResourcesManager.materials = materials;
		ResourcesManager.CubeMesh = CubeMesh;
		ResourcesManager.CapsuleMesh = CapsuleMesh;

        bool[] isTrees = new bool[ 10000 ];
        bool[] isRocks =  new bool[ 10000 ];
        for( int i = 0; i < isTrees.Length; i++ )
        {
            isTrees[ i ] = UnityEngine.Random.value < 0.005f;
            isRocks[ i ] = ! isTrees[ i ] && UnityEngine.Random.value < 0.005f;
        }

        int counter = 0;
        for( int x = 0; x < 100; x++ )
        {
            for( int y = 0; y < 3; y++ )
            {
                for( int z = 0; z < 100; z++ )
                {                    
                    int index = z * 100 + x;
                    int yPosition = y + 0;
                    if( isTrees[ index ] ) yPosition = y + 2;
                    if( isRocks[ index ] ) yPosition = y + 1;
                    //entityManager.SetComponentData( entityArray[ counter ], new MoveSpeedComponent{ moveSpeed = new float2( UnityEngine.Random.Range( -1f,1f ), UnityEngine.Random.Range( -1f,1f ) ) } );

                    entityManager.SetComponentData( entityArray[ counter ], new Translation{ Value = new float3( x, yPosition, z ) } );
                    entityManager.SetSharedComponentData( entityArray[ counter ], new RenderMesh{ mesh = CubeMesh, material = materials[ yPosition ] } );
					if( y == 0 )
					{
						if( isTrees[ index ] || isRocks[ index ] ){
							entityManager.AddComponentData( entityArray[ counter ], new TreeAndRockComponent{ x = x, z = z} );
						}
					}
                    counter++;
                }
            }
        }
        
        entityArray.Dispose();

    }
}
