using Unity.Entities;

[GenerateAuthoringComponent]
public struct ProjectilePrefabData : IComponentData
{
    public Entity Prefab;
}
