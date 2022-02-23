using Unity.Entities;
using Unity.Transforms;

public class ShootingSystem : SystemBase
{
    protected override void OnUpdate()
    {
        ProjectilePrefabData projectilePrefabData = GetSingleton<ProjectilePrefabData>();
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref ShootingData shootingData, in MovementData movementData, in Translation translation) =>
        {
            if (shootingData.IsShooting && shootingData.TimeToShoot <= 0f)
            {
                Entity newProjectile = EntityManager.Instantiate(projectilePrefabData.Prefab);

                EntityManager.SetComponentData(newProjectile, 
                    new MovementData{ Direction = movementData.Forward, Speed = shootingData.ProjectileSpeed});
                
                EntityManager.SetComponentData(newProjectile, 
                    new Translation{ Value = translation.Value + (movementData.Forward * 1.05f)});
                
                shootingData.TimeToShoot = shootingData.ReloadTime;
            }

            shootingData.TimeToShoot -= deltaTime;

        }).WithStructuralChanges().WithoutBurst().Run();
    }
}
