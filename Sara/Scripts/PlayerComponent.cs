using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerComponent : IComponentData
{
    public float3 TargetPosition;
}