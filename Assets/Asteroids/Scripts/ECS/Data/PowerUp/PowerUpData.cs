using Unity.Entities;

[GenerateAuthoringComponent]
public struct PowerUpData : IComponentData
{
    public PowerUpType Type;

    public enum PowerUpType
    {
        None = 0,
        OneShot,
        Shield
    }
}
