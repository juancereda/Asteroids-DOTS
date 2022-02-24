using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(EndFramePhysicsSystem))]
public class CollisionsSystem : JobComponentSystem
{
    private BuildPhysicsWorld _buildPhysicsWorld;
    private StepPhysicsWorld _stepPhysicsWorld;

    private EndSimulationEntityCommandBufferSystem _endSimulationCommandBufferSystem;
    
    private enum CollisionTag
    {
        Projectile,
        Asteroid,
        Player,
        UFO
    }

    protected override void OnCreate()
    {
        _buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        _stepPhysicsWorld = World.GetOrCreateSystem<StepPhysicsWorld>();
        _endSimulationCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
    }

    [BurstCompile]
    struct CollisionsSystemJob : ITriggerEventsJob
    {
        [ReadOnly] public ComponentDataFromEntity<PlayerBehaviourData> AllPlayersBehavioursData;
        [ReadOnly] public ComponentDataFromEntity<AsteroidData> AllAsteroidsData;
        [ReadOnly] public ComponentDataFromEntity<ProjectileTag> AllProjectiles;
        [ReadOnly] public ComponentDataFromEntity<PowerUpData> AllPowerUpData;
        [ReadOnly] public ComponentDataFromEntity<UfoBehaviourData> AllUfosBehavioursData;
        [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslations;

        public EntityCommandBuffer EntityCommandBuffer;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;
            
            
            // Projectiles collisions
            //
            if (AllProjectiles.HasComponent(entityA))
            {
                if (AllAsteroidsData.HasComponent(entityB))
                {
                    Entity asteroidDestroyedData = EntityCommandBuffer.CreateEntity();
                    EntityCommandBuffer.AddComponent(asteroidDestroyedData,
                        new AsteroidDestroyedData
                        {
                            Size = AllAsteroidsData[entityB].Size,
                            Position = AllTranslations[entityB].Value
                        });
                
                    EntityCommandBuffer.DestroyEntity(entityB); // Destroying Asteroid
                    EntityCommandBuffer.DestroyEntity(entityA); // Destroying Projectile
                }

                else if (AllUfosBehavioursData.HasComponent(entityB))
                {
                    if (AllUfosBehavioursData[entityB].IsAlive)
                    {
                        EntityCommandBuffer.AddComponent(entityB, new GotHitTag());
                        EntityCommandBuffer.DestroyEntity(entityA); // Destroying Projectile
                    }
                }
                
                else if (AllPlayersBehavioursData.HasComponent(entityB))
                {
                    if (AllPlayersBehavioursData[entityB].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                        !AllPlayersBehavioursData[entityB].IsUntouchable)
                    {
                        EntityCommandBuffer.AddComponent(entityB, new GotHitTag());
                    }
                    EntityCommandBuffer.DestroyEntity(entityA); // Destroying Projectile
                }

                return;
            }
            
            if (AllProjectiles.HasComponent(entityB))
            {
                if (AllAsteroidsData.HasComponent(entityA))
                {
                    Entity asteroidDestroyedData = EntityCommandBuffer.CreateEntity();
                    EntityCommandBuffer.AddComponent(asteroidDestroyedData,
                        new AsteroidDestroyedData
                        {
                            Size = AllAsteroidsData[entityA].Size,
                            Position = AllTranslations[entityA].Value
                        });
                
                    EntityCommandBuffer.DestroyEntity(entityA); // Destroying Asteroid
                    EntityCommandBuffer.DestroyEntity(entityB); // Destroying Projectile
                }
                
                else if (AllUfosBehavioursData.HasComponent(entityA))
                {
                    if (AllUfosBehavioursData[entityA].IsAlive)
                    {
                        EntityCommandBuffer.AddComponent(entityA, new GotHitTag());
                        EntityCommandBuffer.DestroyEntity(entityB); // Destroying Projectile
                    }
                }
                
                else if (AllPlayersBehavioursData.HasComponent(entityA))
                {
                    if (AllPlayersBehavioursData[entityA].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                        !AllPlayersBehavioursData[entityA].IsUntouchable)
                    {
                        EntityCommandBuffer.AddComponent(entityA, new GotHitTag());
                    }
                    EntityCommandBuffer.DestroyEntity(entityB); // Destroying Projectile
                }

                return;
            }


            // Collision between Player and Asteroids
            //
            if (AllAsteroidsData.HasComponent(entityA) && AllPlayersBehavioursData.HasComponent(entityB))
            {
                if (AllPlayersBehavioursData[entityB].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                    !AllPlayersBehavioursData[entityB].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityB, new GotHitTag());
                }
                return;
            }
            
            if (AllAsteroidsData.HasComponent(entityB) && AllPlayersBehavioursData.HasComponent(entityA))
            {
                if (AllPlayersBehavioursData[entityA].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                    !AllPlayersBehavioursData[entityA].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityA, new GotHitTag());
                }
                return;
            }
            
            
            // Collision between Player and PowerUps
            if (AllPowerUpData.HasComponent(entityA) && AllPlayersBehavioursData.HasComponent(entityB))
            {
                if (AllPlayersBehavioursData[entityB].Status == PlayerBehaviourData.PlayerStatus.Alive)
                {
                    EntityCommandBuffer.AddComponent(entityB, 
                        new PlayerTookPowerUpData{ Type = AllPowerUpData[entityA].Type});

                    EntityCommandBuffer.DestroyEntity(entityA);
                }
                return;
            }
            
            if (AllPowerUpData.HasComponent(entityB) && AllPlayersBehavioursData.HasComponent(entityA))
            {
                if (AllPlayersBehavioursData[entityA].Status == PlayerBehaviourData.PlayerStatus.Alive)
                {
                    EntityCommandBuffer.AddComponent(entityA, 
                        new PlayerTookPowerUpData{ Type = AllPowerUpData[entityB].Type});
                    
                    EntityCommandBuffer.DestroyEntity(entityB);
                }
                return;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CollisionsSystemJob();
        job.AllAsteroidsData = GetComponentDataFromEntity<AsteroidData>(true);
        job.AllPlayersBehavioursData = GetComponentDataFromEntity<PlayerBehaviourData>(true);
        job.AllProjectiles = GetComponentDataFromEntity<ProjectileTag>(true);
        job.AllTranslations = GetComponentDataFromEntity<Translation>(true);
        job.AllPowerUpData = GetComponentDataFromEntity<PowerUpData>(true);
        job.AllUfosBehavioursData = GetComponentDataFromEntity<UfoBehaviourData>(true);
        job.EntityCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle =
            job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
