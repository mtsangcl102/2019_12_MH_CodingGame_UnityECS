using Unity.Entities;
using Unity.Mathematics;

namespace CodingGame
{
    public struct TargetTranslationComponent : IComponentData
    {
        public float2 Direction;
        public int X;
        public int Y;
    }
}