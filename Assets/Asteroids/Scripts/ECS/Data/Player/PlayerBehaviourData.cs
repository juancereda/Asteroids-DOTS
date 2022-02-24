using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerBehaviourData : IComponentData
{
    public float SecondsToRespawn;
    public float SecondsUntouchable;
    public float RespawnTimer;
    public float UntouchableTimer;
    public PlayerStatus Status;
    public bool IsUntouchable;
    public Entity ForceField;
    public PowerUpData.PowerUpType PowerUpAvailable;
    public bool HyperSpaceTravelActivated;

    public enum PlayerStatus
    {
        Alive,
        Respawning
    }
}
