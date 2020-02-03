using Unity.Entities;

namespace CodingGame
{
    public struct BlockComponent : IComponentData
    {
        public BlockType BlockType;
        public int X;
        public int Y;
        public int Z;
    }
}