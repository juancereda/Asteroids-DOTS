using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FadeInSystem : SystemBase {
    
    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        Entities.WithAny<NonUniformScale>().ForEach((Entity entity, int entityInQueryIndex, 
            ref FadeInScaleData fadeInScaleData) =>
        {
            float3 newScale = math.lerp(float3.zero, fadeInScaleData.TargetScale, 
                fadeInScaleData.FadeTimer/fadeInScaleData.FadeDuration);

            beginCommandBuffer.SetComponent(entityInQueryIndex, entity, new NonUniformScale{ Value = newScale });

            fadeInScaleData.FadeTimer += deltaTime;

            if (fadeInScaleData.FadeTimer > fadeInScaleData.FadeDuration)
            {
                beginCommandBuffer.RemoveComponent<FadeInScaleData>(entityInQueryIndex, entity);
            }
        }).ScheduleParallel();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
