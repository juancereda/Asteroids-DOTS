using System.ComponentModel;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class UfoBehaviourSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;
    private Entity _player;

    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnStartRunning() {
        _player = GetEntityQuery(typeof(PlayerTag)).GetSingletonEntity();
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAny<UfoBehaviourData>().ForEach((Entity entity, int entityInQueryIndex) =>
            {
                beginCommandBuffer.AddComponent<DisableRendering>(entityInQueryIndex, entity);
            }).ScheduleParallel();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
    
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        var playerRef = _player;

        // Check if the Ufo Got Hit
        //
        Entities.WithAny<GotHitTag>().ForEach(
            (Entity entity, int entityInQueryIndex, ref UfoBehaviourData ufoBehaviourData, ref Translation translation) =>
            {
                if (ufoBehaviourData.IsAlive)
                {
                    ufoBehaviourData.IsAlive = false;
                    ufoBehaviourData.RespawnTimer = ufoBehaviourData.RespawnDuration;
                    beginCommandBuffer.AddComponent<DisableRendering>(entityInQueryIndex, entity);
                    translation.Value = new float3(1000f, 0f, 0f); // Move it outside the Battlefield
                }

                beginCommandBuffer.RemoveComponent<GotHitTag>(entityInQueryIndex, entity);
            }).ScheduleParallel();
        
        
        
        // Respawn UFO with Timer
        //
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref UfoBehaviourData ufoBehaviourData, 
            ref Translation translation, ref MovementData movementData) =>
        {
            Random random = new Random((uint)((deltaTime + 1f) * 10000f));
            
            if (!ufoBehaviourData.IsAlive)
            {
                if (ufoBehaviourData.RespawnTimer <= 0f)
                {
                    ufoBehaviourData.IsAlive = true;
                    translation.Value = new float3( 22.0f, 0f, random.NextFloat(-10f, 10f));
                    movementData.Direction = new float3( random.NextBool() ? 1f : -1f, 0f, 0f);
                    
                    beginCommandBuffer.RemoveComponent<DisableRendering>(entityInQueryIndex, entity);
                }
                
                ufoBehaviourData.RespawnTimer -= deltaTime;
            }

        }).ScheduleParallel();
        
        
        // Adjust shooting according to player's position
        //
        Entities.ForEach((Entity entity, ref UfoBehaviourData ufoBehaviourData, ref ShootingData shootingData) =>
        {
            var allPlayerBehaviours = GetComponentDataFromEntity<PlayerBehaviourData>(true);
            
            shootingData.IsShooting = ufoBehaviourData.IsAlive 
                                      && allPlayerBehaviours[playerRef].Status == PlayerBehaviourData.PlayerStatus.Alive;

            if (shootingData.IsShooting)
            {
                var allTranslations = GetComponentDataFromEntity<Translation>(true);
                
                shootingData.Direction = math.normalizesafe(allTranslations[playerRef].Value - allTranslations[entity].Value);
            }
        }).ScheduleParallel();
        
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
