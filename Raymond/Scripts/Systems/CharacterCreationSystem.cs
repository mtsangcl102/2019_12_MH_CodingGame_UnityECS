using Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(MapSystem))]
public class CharacterCreationSystem : ComponentSystem
{
    private NativeArray<Translation> _obstacleTranslations;
    private bool _inited = false;
    
    protected override void OnUpdate()
    {
        var gameManager = GameManager.Instance;
        var entityManager = World.Active.EntityManager;

        if ( !_inited )
        {
            _inited = true;
            
            var obstacleEntityQuery = GetEntityQuery(ComponentType.ReadOnly<IsObstacleComponentData>() , ComponentType.ReadOnly<Translation>());
            _obstacleTranslations = obstacleEntityQuery.ToComponentDataArray<Translation>( Allocator.Persistent );
        }
        
        // check the number of entity 
        var entityQuery = GetEntityQuery(ComponentType.ReadOnly<MoveComponentData>());
        var characterCount = entityQuery.CalculateEntityCount();
        var characterLimit = gameManager.CharacterLimit;

        var characterToBeCreated = characterLimit - characterCount;

        if ( characterToBeCreated > 0 )
        {
            // Create a character
            var entityArchetype = entityManager.CreateArchetype(
                typeof(Translation) , typeof(NewPositionComponentData) ,
                typeof(RenderMesh) , typeof(LocalToWorld) , typeof(MoveComponentData) , typeof(IsDestroyedComponentData));
        
            var entityArray = new NativeArray<Entity>( characterToBeCreated , Allocator.Temp );
            entityManager.CreateEntity( entityArchetype , entityArray );

            foreach ( var entity in entityArray )
            {
                var x = UnityEngine.Random.Range( 1 , 99 );
                var z = UnityEngine.Random.Range( 1 , 99 );

                var m = gameManager.CharacterMaterials[
                    UnityEngine.Random.Range( 0 , gameManager.CharacterMaterials.Length )];
                
                // position
                var position = new float3( x , 3.5f , z );
                entityManager.SetComponentData( entity , new Translation{ Value = position} ) ;
                
                // renderer
                entityManager.SetSharedComponentData( entity , new RenderMesh(){
                    mesh = gameManager.CharacterMesh ,
                    material = m
                } );
                
                // random move direction
                var d = UnityEngine.Random.Range( 0 , (int) Direction.COUNT );
                entityManager.SetComponentData( entity , new MoveComponentData(){ Value = CharacterHelper.GetMoveVector( (Direction) d )} );
                
                // is destroyed flag
                entityManager.SetComponentData( entity , new IsDestroyedComponentData(){ Value = false } );
                
                // check if collide with obstacles
                for ( var i = 0 ; i < _obstacleTranslations.Length ; ++i )
                {
                    var obstacle = _obstacleTranslations[i];
                
                    if ( SceneHelper.IsCollide( position , obstacle.Value ) )
                    {
                        PostUpdateCommands.DestroyEntity( entity );
                    }
                }
            }

            entityArray.Dispose();
        }
    }
}