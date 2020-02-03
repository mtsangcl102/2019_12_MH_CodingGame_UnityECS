using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

public class GenerateCharacterSystem : ComponentSystem
{
	bool inited;
	float timePassed;
	EntityArchetype entityArchetype;
	EntityManager entityManager;


	NativeArray<Entity> array;
	TreeAndRockComponent[] treeAndRockComponents = new TreeAndRockComponent[ 0 ];
	EntityQuery m_Group;


    protected override void OnCreate()
    {
		entityManager = World.Active.EntityManager;
		entityArchetype = entityManager.CreateArchetype( typeof(Translation), typeof(RenderMesh), typeof(LocalToWorld), typeof(MoveSpeedComponent) );

		m_Group = GetEntityQuery(typeof(TreeAndRockComponent) );
    }

    protected override void OnUpdate()
    {
		if( ! inited )
		{
			for( int i = 0; i < 5; i++ ){
				CreateCharacter();
			}
			inited = true;

			array = m_Group.ToEntityArray( Allocator.Persistent );
			treeAndRockComponents = new TreeAndRockComponent[ array.Length ];
			EntityManager entityManager = World.Active.EntityManager;
			for( int i = 0; i < treeAndRockComponents.Length; i++ ){
				treeAndRockComponents[ i ] = entityManager.GetComponentData<TreeAndRockComponent>( array[ i ] );
			}
		}
		timePassed += Time.deltaTime;
		if( timePassed > 10f )
		{
			CreateCharacter();
			timePassed = 0f;
		}
    }


	void CreateCharacter()
	{
		Entity entity = EntityManager.CreateEntity( entityArchetype );

		int x = UnityEngine.Random.Range( 5, 95 );
		int y = UnityEngine.Random.Range( 5, 95 );

		for( int i = 0; i < treeAndRockComponents.Length; i++ )
		{
			float distanceX = treeAndRockComponents[ i ].x - x;
			float distanceZ = treeAndRockComponents[ i ].z - y;
			if( distanceX < 0 ) distanceX *= -1;
			if( distanceZ < 0 ) distanceZ *= -1;
			if( distanceX < 0.98f && distanceZ < 0.1f ){
				return;
			}
			if( distanceZ < 0.98f && distanceX < 0.1f ){
				return;
			}
		}

        entityManager.SetComponentData( entity, new Translation{ Value = new float3( x, 3, y ) } );

		float speed = UnityEngine.Random.value < 0.5f ? 40f : -40f;

		entityManager.SetComponentData( entity, new MoveSpeedComponent{ moveSpeed = UnityEngine.Random.value < 0.5f ? new float2( speed, 0f ) : new float2( 0f, speed )} );
        entityManager.SetSharedComponentData( entity, new RenderMesh{ mesh = ResourcesManager.CapsuleMesh, material = ResourcesManager.materials[ 0 ] } );
	}
}
