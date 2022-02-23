using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class AsteroidsSpawnerSystem : SystemBase
{
    private bool _spawned = false;
    private Random _random;
    private BeginInitializationEntityCommandBufferSystem _beginInitCommandBufferSystem;
    private EndSimulationEntityCommandBufferSystem _endSimulationCommandBufferSystem;

    protected override void OnCreate()
    {
        _random = new Random((uint)(1.0f + Time.ElapsedTime));
        _endSimulationCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        _beginInitCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var begginCommandBuffeer = _beginInitCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var endCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        AsteroidPrefabData asteroidPrefabData = GetSingleton<AsteroidPrefabData>();

        float deltaTime = Time.DeltaTime;
        
        if (!_spawned)
        {
            for (int i = 0; i < 10; i++)
            {
                int asteroidSize = _random.NextInt(1, asteroidPrefabData.MaxSize + 1);
                float3 asteroidPosition =
                    new float3(_random.NextFloat(-10.0f, 10.0f), 0.0f, _random.NextFloat(-10.0f, 10.0f));
                
                
                Entity newAsteroid = EntityManager.Instantiate(asteroidPrefabData.Prefab);

                EntityManager.SetComponentData(newAsteroid, new Translation {Value = asteroidPosition});
                EntityManager.SetComponentData(newAsteroid, new AsteroidData() { Size = asteroidSize});
                EntityManager.AddComponent<NonUniformScale>(newAsteroid);
                EntityManager.SetComponentData(newAsteroid, 
                    new NonUniformScale{ Value = new float3(asteroidSize * 0.5f)});
                EntityManager.SetComponentData(newAsteroid, 
                    new MovementData
                    {
                        Direction = math.normalizesafe( new float3(_random.NextFloat(-1f, 1f),0.0f,_random.NextFloat(-1f, 1f))),
                        Speed = 4.3f - asteroidSize
                    });
                
                EntityManager.SetComponentData(newAsteroid, 
                    new ConstantRotationData()
                    {
                        Speed = 4.3f - asteroidSize,
                        Angle = new float3(_random.NextFloat(0f, 1f),0f,_random.NextFloat(0f, 1f))
                    });
            }
            
            _spawned = true;
        }
        
        
        // Asteroid has been destroyed. Spawn two more if the Asteroid is big enough
        //
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref AsteroidDestroyedData asteroidDestroyedData) =>
        {
            if (asteroidDestroyedData.Size > 1)
            {
                Random random = new Random((uint)(1f + deltaTime));
                
                int asteroidSize = asteroidDestroyedData.Size - 1;

                for (int i = 0; i < 2; i++)
                {
                    Entity newAsteroid = begginCommandBuffeer.Instantiate(entityInQueryIndex, asteroidPrefabData.Prefab);

                    
                    begginCommandBuffeer.SetComponent(entityInQueryIndex, newAsteroid, 
                        new Translation {Value = asteroidDestroyedData.Position});
                    begginCommandBuffeer.SetComponent(entityInQueryIndex, newAsteroid, 
                        new AsteroidData() { Size = asteroidSize});
                    begginCommandBuffeer.AddComponent<NonUniformScale>(entityInQueryIndex, newAsteroid);
                    begginCommandBuffeer.SetComponent(entityInQueryIndex, newAsteroid, 
                        new NonUniformScale{ Value = new float3(asteroidSize * 0.5f)});
                    begginCommandBuffeer.SetComponent(entityInQueryIndex, newAsteroid, 
                        new MovementData
                        {
                            Direction = math.normalizesafe( new float3(random.NextFloat(-1f, 1f),0.0f,random.NextFloat(-1f, 1f))),
                            Speed = 4.3f - asteroidSize
                        });
                
                    begginCommandBuffeer.SetComponent(entityInQueryIndex, newAsteroid, 
                        new ConstantRotationData()
                        {
                            Speed = 4.3f - asteroidSize,
                            Angle = new float3(random.NextFloat(0f, 1f),0f,random.NextFloat(0f, 1f))
                        });
                }
            }
            
            endCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(Dependency);
        _beginInitCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
