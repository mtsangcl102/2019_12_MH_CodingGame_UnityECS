using Unity.Entities;

public struct Collider : IComponentData
{
    public float Size;
    public BodyType bodyType;
    
    public enum BodyType {
        Terrian,
        Soldier
    }
}