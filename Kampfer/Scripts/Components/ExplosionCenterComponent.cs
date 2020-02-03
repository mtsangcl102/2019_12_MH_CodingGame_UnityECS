using Unity.Entities;
using Unity.Mathematics;

namespace CodingGame
{
    public struct ExplosionCenterComponent : IComponentData
    {
        public float3 Position;
        public int MaterialIndex;
    }
}