using Unity.Entities;

[GenerateAuthoringComponent]
public struct ProjectilePrefabData : IComponentData
{
    public Entity PlayerProjectile;
    public Entity EnemyProjectile;
}
