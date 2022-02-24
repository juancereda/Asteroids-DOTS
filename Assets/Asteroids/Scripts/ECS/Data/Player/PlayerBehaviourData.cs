using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerBehaviourData : IComponentData
{
    public float SecondsToRespawn;
    public float SecondsUntouchableAtRespawn;
    public float RespawnTimer;
    public float UntouchableTimer;
    public PlayerStatus Status;
    public bool IsUntouchable;
    public Entity ForceField;
    public PowerUpData.PowerUpType CurrentPowerUp;
    public bool HyperSpaceTravelActivated;
    public float PowerUpInvulnerableDuration;
    public float PowerUpOneShotDuration;
    public float OneShootTimer;

    public enum PlayerStatus
    {
        Alive,
        Respawning
    }
}
