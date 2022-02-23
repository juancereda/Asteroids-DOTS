using Unity.Entities;

[GenerateAuthoringComponent]
public struct ShootingData : IComponentData
{
    public bool IsShooting;
    public float TimeToShoot;
    public float ReloadTime;
    public float ProjectileSpeed;
}
