using Unity.Entities;
using Unity.Mathematics;

namespace CodingGame
{
    public struct VelocityComponent : IComponentData
    {
        public float3 Value;
    }
}