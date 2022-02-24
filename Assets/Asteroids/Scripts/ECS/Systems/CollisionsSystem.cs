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
        [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslations;

        public EntityCommandBuffer EntityCommandBuffer;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            // Collision between Asteroids and Projectile
            //
            if (AllAsteroidsData.HasComponent(entityA) && AllProjectiles.HasComponent(entityB))
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
                return;
            }
            
            if (AllAsteroidsData.HasComponent(entityB) && AllProjectiles.HasComponent(entityA))
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
                return;
            }
            
            
            // Collision between Player and Asteroids
            //
            if (AllAsteroidsData.HasComponent(entityA) && AllPlayersBehavioursData.HasComponent(entityB))
            {
                if (AllPlayersBehavioursData[entityB].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                    !AllPlayersBehavioursData[entityB].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityB, new PlayerGotHitData());
                }
                return;
            }
            
            if (AllAsteroidsData.HasComponent(entityB) && AllPlayersBehavioursData.HasComponent(entityA))
            {
                if (AllPlayersBehavioursData[entityA].Status == PlayerBehaviourData.PlayerStatus.Alive &&
                    !AllPlayersBehavioursData[entityA].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityA, new PlayerGotHitData());
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
        job.EntityCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle =
            job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
