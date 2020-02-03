using Unity.Entities;
using Unity.Mathematics;

public struct RigiBody : IComponentData
{
    public float2 Velocity;
    public bool collied;
    public bool colliedWithSoldier;
    public bool colliedEnter;
}