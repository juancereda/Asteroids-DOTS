using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;

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
        [ReadOnly] public ComponentDataFromEntity<AsteroidTag> AllAsteroids;
        [ReadOnly] public ComponentDataFromEntity<ProjectileTag> AllProjectiles;

        public EntityCommandBuffer EntityCommandBuffer;
        
        public void Execute(TriggerEvent triggerEvent)
        {
            Entity entityA = triggerEvent.EntityA;
            Entity entityB = triggerEvent.EntityB;

            if (AllAsteroids.HasComponent(entityA) && AllProjectiles.HasComponent(entityB))
            {
                EntityCommandBuffer.DestroyEntity(entityA);
                EntityCommandBuffer.DestroyEntity(entityB);
            }
            
            else if (AllAsteroids.HasComponent(entityB) && AllProjectiles.HasComponent(entityA))
            {
                EntityCommandBuffer.DestroyEntity(entityA);
                EntityCommandBuffer.DestroyEntity(entityB);
            }
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new CollisionsSystemJob();
        job.AllAsteroids = GetComponentDataFromEntity<AsteroidTag>(true);
        job.AllPlayers = GetComponentDataFromEntity<PlayerTag>(true);
        job.AllProjectiles = GetComponentDataFromEntity<ProjectileTag>(true);
        job.EntityCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer();

        JobHandle jobHandle =
            job.Schedule(_stepPhysicsWorld.Simulation, ref _buildPhysicsWorld.PhysicsWorld, inputDeps);
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(jobHandle);
        return jobHandle;
    }
}
