using System;
using Unity.Burst;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEditor.Experimental.GraphView;

namespace CodingGame
{
    public class PlayerTargetDebugSystem : ComponentSystem {

        protected override void OnUpdate() {
            Entities.ForEach((Entity entity, ref PlayerComponent player, ref Translation translation, ref TargetTranslationComponent targetTranslation) => {
                Debug.DrawLine(translation.Value, new Vector3(targetTranslation.X, translation.Value.y, targetTranslation.Y));
            });
        }
    }
}