using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerHealthData : IComponentData
{
    public float SecondsToRespawn;
    public float SecondsUntouchable;
    public float RespawnTimer;
    public float UntouchableTimer;
    public PlayerStatus Status;
    public bool AsteroidCollision;
    public bool IsUntouchable;
    public Entity ForceField;

    public enum PlayerStatus
    {
        Alive,
        Respawning
    }
}
