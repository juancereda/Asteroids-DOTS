using Unity.Entities;

[GenerateAuthoringComponent]
public struct PowerUpPrefabData : IComponentData
{
    public Entity OneShotPrefab;
    public Entity ShieldPrefab;
    public float TimeToSpawn;
}
