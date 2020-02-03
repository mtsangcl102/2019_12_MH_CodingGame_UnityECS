using Helpers;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

[UpdateAfter(typeof(CharacterUpdateSystem))]
public class CharacterDestroySystem : ComponentSystem
{
    private EntityQuery _characters;
    
    protected override void OnCreate()
    {
    }

    protected override void OnUpdate()
    {
        _characters = GetEntityQuery(ComponentType.ReadOnly<Translation>(),ComponentType.ReadOnly<MoveComponentData>());
        
        var translationArray = _characters.ToComponentDataArray<Translation>( Allocator.TempJob );
        var entityArray = _characters.ToEntityArray( Allocator.TempJob );

        for ( var i = 0 ; i < translationArray.Length - 1 ; ++i )
        {
            for ( var j = i + 1 ; j < translationArray.Length ; ++j )
            {
                var entityA = translationArray[i];
                var entityB = translationArray[j];

                if ( SceneHelper.IsCollide( entityA.Value , entityB.Value ) )
                {
                    PostUpdateCommands.DestroyEntity( entityArray[i] );
                }
            }
        }
        
        translationArray.Dispose();
        entityArray.Dispose();
    }
}