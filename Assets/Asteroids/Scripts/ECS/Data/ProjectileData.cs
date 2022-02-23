using Unity.Entities;

[GenerateAuthoringComponent]
public struct ProjectileData : IComponentData
{
    public float DestroyTimer;
}
