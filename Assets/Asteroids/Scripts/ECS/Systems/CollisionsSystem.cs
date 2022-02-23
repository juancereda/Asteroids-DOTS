using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
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
        [ReadOnly] public ComponentDataFromEntity<PlayerTag> AllPlayers;
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
                CollisionBetweenAsteroidAndProjectile(ref EntityCommandBuffer, ref AllAsteroidsData, ref AllTranslationData, 
                    projectile: entityB, asteroid: entityA);
            }
            else if (AllAsteroidsData.HasComponent(entityB) && AllProjectiles.HasComponent(entityA))
            {
                CollisionBetweenAsteroidAndProjectile(ref EntityCommandBuffer, ref AllAsteroidsData, ref AllTranslationData, 
                    projectile: entityA, asteroid: entityB);
            }
            
            
            // Collision between Player and Asteroids
            //
            if (AllAsteroidsData.HasComponent(entityA) && AllPlayers.HasComponent(entityB))
            {
            }
            else if (AllAsteroidsData.HasComponent(entityB) && AllPlayers.HasComponent(entityA))
            {
            }
            
            

            static void CollisionBetweenAsteroidAndProjectile(
                ref EntityCommandBuffer commandBuffer, 
                ref ComponentDataFromEntity<AsteroidData> allAsteroidsData,
                ref ComponentDataFromEntity<Translation> allTranslationsData,
                Entity projectile, 
                Entity asteroid)
            {
                Entity asteroidDestroyed = commandBuffer.CreateEntity();
                commandBuffer.AddComponent(asteroidDestroyed,
                    new AsteroidDestroyedData
                    {
                        Size = allAsteroidsData[asteroid].Size,
                        Position = allTranslationsData[asteroid].Value
                    });
                
                commandBuffer.DestroyEntity(asteroid); // Destroying Asteroid
                commandBuffer.DestroyEntity(projectile); // Destroying Projectile
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CollisionsSystemJob();
        job.AllAsteroidsData = GetComponentDataFromEntity<AsteroidData>(true);
        job.AllPlayers = GetComponentDataFromEntity<PlayerTag>(true);
        job.AllProjectiles = GetComponentDataFromEntity<ProjectileTag>(true);
        job.AllTranslationData = GetComponentDataFromEntity<Translation>(true);
        job.EntityCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle =
            job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
