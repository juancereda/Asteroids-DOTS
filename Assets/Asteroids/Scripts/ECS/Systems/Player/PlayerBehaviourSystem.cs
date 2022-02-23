using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class PlayerBehaviourSystem : SystemBase {

    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate() {
        float deltaTime = Time.DeltaTime;
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        
        
        Entities.WithAny<PlayerGotHitData>().
            ForEach((Entity entity, int entityInQueryIndex, ref PlayerBehaviourData playerBehaviourData) =>
        {
            if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Alive)
            {
                playerBehaviourData.Status = PlayerBehaviourData.PlayerStatus.Respawning;
                playerBehaviourData.RespawnTimer = playerBehaviourData.SecondsToRespawn;

                beginCommandBuffer.AddComponent<DisableRendering>(entityInQueryIndex, entity);
            }
            
            beginCommandBuffer.RemoveComponent<PlayerGotHitData>(entityInQueryIndex, entity);
        }).Schedule();
        
        
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PlayerBehaviourData playerBehaviourData,
            ref Translation translation, ref MovementData movementData, ref Rotation rotation) => {

            if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Respawning)
            {
                if (playerBehaviourData.RespawnTimer <= 0f)
                {
                    playerBehaviourData.IsUntouchable = true;
                    playerBehaviourData.UntouchableTimer = playerBehaviourData.SecondsUntouchable;
                    playerBehaviourData.Status = PlayerBehaviourData.PlayerStatus.Alive;
                    beginCommandBuffer.RemoveComponent<DisableRendering>(entityInQueryIndex, entity);
                    beginCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerBehaviourData.ForceField);
                    translation.Value = float3.zero;
                    movementData.Direction = float3.zero;
                    movementData.Forward = float3.zero;
                    rotation.Value = quaternion.LookRotationSafe(float3.zero, math.up());
                }
                else
                {
                    playerBehaviourData.RespawnTimer -= deltaTime;
                }
            }
            else
            {
                if (playerBehaviourData.IsUntouchable)
                {
                    if (playerBehaviourData.UntouchableTimer <= 0f)
                    {
                        playerBehaviourData.IsUntouchable = false;
                    }
                    else
                    {
                        playerBehaviourData.UntouchableTimer -= deltaTime;
                    }
                }
                
                
                if (playerBehaviourData.IsUntouchable)
                {
                    beginCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerBehaviourData.ForceField);
                }
                else
                {
                    beginCommandBuffer.AddComponent<Disabled>(entityInQueryIndex, playerBehaviourData.ForceField);
                }
            }

        }).Schedule();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}