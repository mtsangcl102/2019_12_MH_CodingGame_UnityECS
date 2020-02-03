using Unity.Entities;
using Unity.Mathematics;

public struct AABBComponent : IComponentData {
    public float bound;
    // status 0 == nothing
    // status 1 == terrain
    // status 2 == character
    public int status;
}
