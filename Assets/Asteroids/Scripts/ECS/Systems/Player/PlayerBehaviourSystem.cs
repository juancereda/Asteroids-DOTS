using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

public class PlayerBehaviourSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;
    
    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }
    
    protected override void OnUpdate() {
        float deltaTime = Time.DeltaTime;
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        
        // Check if the player Got Hit
        //
        Entities.WithAny<PlayerGotHitData>().ForEach(
            (Entity entity, int entityInQueryIndex, ref PlayerBehaviourData playerBehaviourData) =>
            {
                if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Alive)
                {
                    playerBehaviourData.Status = PlayerBehaviourData.PlayerStatus.Respawning;
                    playerBehaviourData.RespawnTimer = playerBehaviourData.SecondsToRespawn;
                    playerBehaviourData.CurrentPowerUp = PowerUpData.PowerUpType.None;

                    beginCommandBuffer.AddComponent<DisableRendering>(entityInQueryIndex, entity);
                }

                beginCommandBuffer.RemoveComponent<PlayerGotHitData>(entityInQueryIndex, entity);
            }).ScheduleParallel();

        
        // Check if the player got a PowerUp
        //
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PlayerBehaviourData playerBehaviourData,
            ref PlayerTookPowerUpData playerTookPowerUpData) =>
        {
            playerBehaviourData.CurrentPowerUp = playerTookPowerUpData.Type;

            if (playerTookPowerUpData.Type == PowerUpData.PowerUpType.Shield)
            {
                playerBehaviourData.IsUntouchable = true;
                playerBehaviourData.UntouchableTimer = playerBehaviourData.PowerUpInvulnerableDuration;
            }
            
            else if (playerTookPowerUpData.Type == PowerUpData.PowerUpType.OneShot)
            {
                playerBehaviourData.OneShootTimer = playerBehaviourData.PowerUpOneShotDuration;
            }

            beginCommandBuffer.RemoveComponent<PlayerTookPowerUpData>(entityInQueryIndex, entity);
        }).ScheduleParallel();
        
        
        // Main behaviour Update
        //
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref PlayerBehaviourData playerBehaviourData,
            ref Translation translation, ref MovementData movementData, ref Rotation rotation, ref ShootingData shootingData) => {

            if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Respawning)
            {
                
                if (playerBehaviourData.RespawnTimer <= 0f)
                {
                    // Respawn player with a shield
                    //
                    playerBehaviourData.IsUntouchable = true;
                    playerBehaviourData.UntouchableTimer = playerBehaviourData.SecondsUntouchableAtRespawn;
                    playerBehaviourData.Status = PlayerBehaviourData.PlayerStatus.Alive;
                    beginCommandBuffer.RemoveComponent<DisableRendering>(entityInQueryIndex, entity);
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
            else // player is alive
            {
                if (playerBehaviourData.IsUntouchable)
                {
                    if (playerBehaviourData.UntouchableTimer <= 0f)
                    {
                        playerBehaviourData.IsUntouchable = false;

                        if (playerBehaviourData.CurrentPowerUp == PowerUpData.PowerUpType.Shield)
                        {
                            playerBehaviourData.CurrentPowerUp = PowerUpData.PowerUpType.None;
                        }
                    }
                    else
                    {
                        playerBehaviourData.UntouchableTimer -= deltaTime;
                    }
                }

                Random random = new Random((uint)((deltaTime + 1f) * 10000f));

                if (playerBehaviourData.HyperSpaceTravelActivated)
                {
                    translation.Value = new float3(random.NextFloat(-20f, 20f), 0f, random.NextFloat(-10f, 10f));
                    playerBehaviourData.HyperSpaceTravelActivated = false;
                }
                

                if (playerBehaviourData.CurrentPowerUp == PowerUpData.PowerUpType.OneShot)
                {
                    shootingData.CurrentReloadTime = 0.05f;

                    if (playerBehaviourData.OneShootTimer <= 0f)
                    {
                        playerBehaviourData.CurrentPowerUp = PowerUpData.PowerUpType.None;
                        shootingData.CurrentReloadTime = shootingData.ReloadTime;
                    }

                    playerBehaviourData.OneShootTimer -= deltaTime;
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

        }).ScheduleParallel();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
