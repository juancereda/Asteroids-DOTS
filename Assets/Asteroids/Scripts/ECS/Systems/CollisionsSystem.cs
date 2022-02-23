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
        [ReadOnly] public ComponentDataFromEntity<PlayerHealthData> AllPlayersHealthData;
        [ReadOnly] public ComponentDataFromEntity<AsteroidData> AllAsteroidsData;
        [ReadOnly] public ComponentDataFromEntity<ProjectileTag> AllProjectiles;
        [ReadOnly] public ComponentDataFromEntity<Translation> AllTranslationData;

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
                        Position = AllTranslationData[entityA].Value
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
                        Position = AllTranslationData[entityB].Value
                    });
                
                EntityCommandBuffer.DestroyEntity(entityB); // Destroying Asteroid
                EntityCommandBuffer.DestroyEntity(entityA); // Destroying Projectile
                return;
            }
            
            
            // Collision between Player and Asteroids
            //
            if (AllAsteroidsData.HasComponent(entityA) && AllPlayersHealthData.HasComponent(entityB))
            {
                if (!AllPlayersHealthData[entityB].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityB, new PlayerGotHitData());
                }
                return;
            }
            
            if (AllAsteroidsData.HasComponent(entityB) && AllPlayersHealthData.HasComponent(entityA))
            {
                if (!AllPlayersHealthData[entityA].IsUntouchable)
                {
                    EntityCommandBuffer.AddComponent(entityA, new PlayerGotHitData());
                }
                return;
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CollisionsSystemJob();
        job.AllAsteroidsData = GetComponentDataFromEntity<AsteroidData>(true);
        job.AllPlayersHealthData = GetComponentDataFromEntity<PlayerHealthData>(true);
        job.AllProjectiles = GetComponentDataFromEntity<ProjectileTag>(true);
        job.AllTranslationData = GetComponentDataFromEntity<Translation>(true);
        job.EntityCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle =
            job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
