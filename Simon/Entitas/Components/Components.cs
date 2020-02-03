using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct PlayerComponent : IComponentData
{
    public Vector2 Direction;
    public float Speed;
    public float CheckTime;
}

public struct TerrainComponent : IComponentData
{
    public float Height;
    public Enum.TerrainType Type;
}