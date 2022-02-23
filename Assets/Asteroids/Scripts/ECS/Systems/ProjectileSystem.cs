using Unity.Entities;

public class ProjectileSystem : SystemBase
{
    private EndSimulationEntityCommandBufferSystem _entityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _entityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        var commandBuffer = _entityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref ProjectileData projectileData) => {
            if (projectileData.DestroyTimer <= 0f)
            {
                commandBuffer.DestroyEntity(entityInQueryIndex, entity);
            }

            projectileData.DestroyTimer -= deltaTime;

        }).ScheduleParallel();
        
        _entityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
