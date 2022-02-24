using Unity.Entities;

public struct PlayerTookPowerUpData : IComponentData
{
    public PowerUpData.PowerUpType Type;
}
