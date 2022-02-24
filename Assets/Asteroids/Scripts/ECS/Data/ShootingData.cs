using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ShootingData : IComponentData
{
    public bool IsShooting;
    public bool IsPlayer;
    public float TimeToShoot;
    public float ReloadTime;
    public float CurrentReloadTime;
    public float ProjectileSpeed;
    public float3 Direction;
}
