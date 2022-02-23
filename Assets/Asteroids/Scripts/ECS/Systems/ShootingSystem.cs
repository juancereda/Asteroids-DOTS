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
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref ShootingData shootingData,  in MovementData movementData, in Translation translation) =>
        {
            if (shootingData.IsShooting && shootingData.TimeToShoot <= 0f)
            {
                Entity newProjectile = commandBuffer.Instantiate(entityInQueryIndex, projectilePrefabData.Prefab);
                
                commandBuffer.SetComponent(entityInQueryIndex, newProjectile, 
                    new MovementData{ Direction = movementData.Forward, Speed = shootingData.ProjectileSpeed});

                commandBuffer.SetComponent(entityInQueryIndex, newProjectile, 
                    new Translation{ Value = translation.Value + movementData.Forward * 0.5f});
                
                shootingData.TimeToShoot = shootingData.ReloadTime;
            }

            shootingData.TimeToShoot -= deltaTime;

        }).ScheduleParallel();
        
        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
