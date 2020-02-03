using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;
using Unity.Mathematics;

public class MoverSystem : ComponentSystem
{
	NativeArray<Entity> array;
	TreeAndRockComponent[] treeAndRockComponents = new TreeAndRockComponent[ 0 ];
	EntityQuery m_Group;
	bool inited;

	List<float2> characterPositions = new List<float2>(); 
	List<Entity> characterEntities = new List<Entity>();

	protected override void OnCreate()
	{
		m_Group = GetEntityQuery(typeof(TreeAndRockComponent) );
		
	}

	protected override void OnDestroy()
	{
		array.Dispose();
	}

	protected override void OnUpdate()
    {
		if( ! inited )
		{
			array = m_Group.ToEntityArray( Allocator.Persistent );
			treeAndRockComponents = new TreeAndRockComponent[ array.Length ];
			EntityManager entityManager = World.Active.EntityManager;
			for( int i = 0; i < treeAndRockComponents.Length; i++ ){
				treeAndRockComponents[ i ] = entityManager.GetComponentData<TreeAndRockComponent>( array[ i ] );
			}
			inited = true;
		}

		characterPositions.Clear();
		characterEntities.Clear();

        Entities.ForEach( 
			( Entity entity, ref Translation translation, ref MoveSpeedComponent m )=>
			{ 				
				float x = translation.Value.x + Time.deltaTime * m.moveSpeed.x;
				float z = translation.Value.z + Time.deltaTime * m.moveSpeed.y; 

				translation.Value.x = x;
				translation.Value.z = z;
				characterPositions.Add( new float2( x, z ) );
				characterEntities.Add( entity );
				for( int i = 0; i < treeAndRockComponents.Length; i++ )
				{
					float distanceX = treeAndRockComponents[ i ].x - x;
					float distanceZ = treeAndRockComponents[ i ].z - z;
					if( distanceX < 0 ) distanceX *= -1;
					if( distanceZ < 0 ) distanceZ *= -1;
					if( distanceX < 0.98f && distanceZ < 0.1f ){
						m.moveSpeed.x *= -1f;
					}
					if( distanceZ < 0.98f && distanceX < 0.1f ){
						m.moveSpeed.y *= -1f;
					}
				}

				if( x < 0 ) m.moveSpeed.x = 40f;
				else if( x > 99f )m.moveSpeed.x = -40f;
				if( z < 0 ) m.moveSpeed.y = 40f;
				else if( z > 99f ) m.moveSpeed.y = -40f;
			}		
		);

		for( int i = 0; i < characterEntities.Count; i++ )
		{ 	
			for( int j = 0; j < characterPositions.Count; j++ )
			{
				if( i == j ) continue;
				float distanceX = characterPositions[ i ].x - characterPositions[ j ].x;
				float distanceZ = characterPositions[ i ].y - characterPositions[ j ].y;
				if( distanceX < 0 ) distanceX *= -1;
				if( distanceZ < 0 ) distanceZ *= -1;
				
				if( (distanceX < 0.98f && distanceZ < 0.1f ) || ( distanceZ < 0.98f && distanceX < 0.1f ) )
				{
					World.Active.EntityManager.DestroyEntity( characterEntities[ i ] );
				}
			}
		}
    }
}
