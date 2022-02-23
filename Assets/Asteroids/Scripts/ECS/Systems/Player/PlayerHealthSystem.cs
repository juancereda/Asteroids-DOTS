using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public class PlayerHealthSystem : SystemBase {

    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate() {
        float deltaTime = Time.DeltaTime;
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        
        
        
        Entities.WithAny<PlayerGotHitData>().
            ForEach((Entity entity, int entityInQueryIndex, ref PlayerHealthData playerHealthData) =>
        {
            if (playerHealthData.Status == PlayerHealthData.PlayerStatus.Alive)
            {
                playerHealthData.Status = PlayerHealthData.PlayerStatus.Respawning;
                playerHealthData.RespawnTimer = playerHealthData.SecondsToRespawn;

                beginCommandBuffer.AddComponent<DisableRendering>(entityInQueryIndex, entity);
            }
            
            beginCommandBuffer.RemoveComponent<PlayerGotHitData>(entityInQueryIndex, entity);
        }).Schedule();
        
        
        
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PlayerHealthData playerHealthData,
            ref Translation translation, ref MovementData movementData, ref Rotation rotation) => {

            if (playerHealthData.Status == PlayerHealthData.PlayerStatus.Respawning)
            {
                if (playerHealthData.RespawnTimer <= 0f)
                {
                    playerHealthData.IsUntouchable = true;
                    playerHealthData.UntouchableTimer = playerHealthData.SecondsUntouchable;
                    playerHealthData.Status = PlayerHealthData.PlayerStatus.Alive;
                    beginCommandBuffer.RemoveComponent<DisableRendering>(entityInQueryIndex, entity);
                    beginCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerHealthData.ForceField);
                    translation.Value = float3.zero;
                    movementData.Direction = float3.zero;
                    movementData.Forward = float3.zero;
                    rotation.Value = quaternion.LookRotationSafe(float3.zero, math.up());
                }
                else
                {
                    playerHealthData.RespawnTimer -= deltaTime;
                }
            }
            else
            {
                if (playerHealthData.IsUntouchable)
                {
                    if (playerHealthData.UntouchableTimer <= 0f)
                    {
                        playerHealthData.IsUntouchable = false;
                    }
                    else
                    {
                        playerHealthData.UntouchableTimer -= deltaTime;
                    }
                }
                
                
                if (playerHealthData.IsUntouchable)
                {
                    beginCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerHealthData.ForceField);
                }
                else
                {
                    beginCommandBuffer.AddComponent<Disabled>(entityInQueryIndex, playerHealthData.ForceField);
                }
            }

        }).Schedule();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
