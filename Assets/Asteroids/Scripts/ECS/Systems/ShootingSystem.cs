using Unity.Entities;
using Unity.Transforms;

public class ShootingSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _entityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        ProjectilePrefabData projectilePrefabData = GetSingleton<ProjectilePrefabData>();
        float deltaTime = Time.DeltaTime;

        var commandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref ShootingData shootingData, in Translation translation) =>
        {
            if (shootingData.IsShooting && shootingData.TimeToShoot <= 0f)
            {
                Entity newProjectile = commandBuffer.Instantiate(entityInQueryIndex, 
                    shootingData.IsPlayer? projectilePrefabData.PlayerProjectile : projectilePrefabData.EnemyProjectile);
                
                commandBuffer.SetComponent(entityInQueryIndex, newProjectile, 
                    new MovementData{ Direction = shootingData.Direction, Speed = shootingData.ProjectileSpeed});

                commandBuffer.SetComponent(entityInQueryIndex, newProjectile, 
                    new Translation{ Value = translation.Value + shootingData.Direction * 0.5f});
                
                shootingData.TimeToShoot = shootingData.CurrentReloadTime;
            }

            shootingData.TimeToShoot -= deltaTime;

        }).ScheduleParallel();
        
        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
