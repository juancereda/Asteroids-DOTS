using Unity.Entities;

[GenerateAuthoringComponent]
public struct AsteroidPrefabData : IComponentData
{
    public Entity Prefab;
    public int MaxSize;
}
