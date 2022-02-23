using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class AsteroidsSpawnerSystem : SystemBase
{
    private bool _spawnedAtStart;
    private float _spawnCountDown = 5.0f;
    private Random _random;
    private BeginInitializationEntityCommandBufferSystem _beginInitCommandBufferSystem;
    private EndSimulationEntityCommandBufferSystem _endSimulationCommandBufferSystem;

    protected override void OnCreate()
    {
        _random = new Random(123);
        _endSimulationCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        _beginInitCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var beginCommandBuffer = _beginInitCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var endCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        AsteroidPrefabData asteroidPrefabData = GetSingleton<AsteroidPrefabData>();

        double elapsedTime = Time.ElapsedTime;

        if (!_spawnedAtStart)
        {
            for (int i = 0; i < 10; i++)
            {
                SpawnAsteroidAtBorders();
            }

            _spawnCountDown = asteroidPrefabData.SpawnTimer;
            _spawnedAtStart = true;
        }
        else if (_spawnCountDown < 0.0f)
        {
            SpawnAsteroidAtBorders();
            _spawnCountDown = asteroidPrefabData.SpawnTimer;
        }

        _spawnCountDown -= Time.DeltaTime;


        // Asteroid has been destroyed. 
        //
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref AsteroidDestroyedData asteroidDestroyedData) =>
        {
            Random random = new Random((uint) (elapsedTime * 1000f));
            
            if (asteroidDestroyedData.Size > 1) // If true spawn two more
            {

                int asteroidSize = asteroidDestroyedData.Size - 1;

                for (int i = 0; i < 2; i++)
                {
                    Entity newAsteroid = beginCommandBuffer.Instantiate(entityInQueryIndex, asteroidPrefabData.Prefab);


                    beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                        new Translation {Value = asteroidDestroyedData.Position});
                    beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                        new AsteroidData() {Size = asteroidSize});
                    beginCommandBuffer.AddComponent<NonUniformScale>(entityInQueryIndex, newAsteroid);
                    beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                        new NonUniformScale {Value = new float3(asteroidSize * 0.5f)});

                    
                    beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                        new MovementData
                        {
                            Direction = math.normalizesafe(new float3(random.NextFloat(-1f, 1f), 0.0f,
                                random.NextFloat(-1f, 1f))),
                            Speed = 4.3f - asteroidSize
                        });

                    beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                        new ConstantRotationData()
                        {
                            Speed = 4.3f - asteroidSize,
                            Angle = new float3(random.NextFloat(0f, 1f), 0f, random.NextFloat(0f, 1f))
                        });
                }
            }
            else if(random.NextInt(0,4) == 3) // Spawn one asteroid at the borders here and there.
            {

                int asteroidSize = random.NextInt(2, asteroidPrefabData.MaxSize + 1);

                float xSide = random.NextBool() ? 1f : -1f;
                float zSide = random.NextBool() ? 1f : -1f;

                float3 asteroidPosition = new float3(
                    random.NextFloat(0f, 21f) * xSide,
                    0.0f,
                    12.5f * zSide);

                Entity newAsteroid = beginCommandBuffer.Instantiate(entityInQueryIndex, asteroidPrefabData.Prefab);

                beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                    new Translation {Value = asteroidPosition});
                beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                    new AsteroidData() {Size = asteroidSize});
                beginCommandBuffer.AddComponent<NonUniformScale>(entityInQueryIndex, newAsteroid);
                beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                    new NonUniformScale {Value = new float3(asteroidSize * 0.5f)});
                beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                    new MovementData
                    {
                        Direction = math.normalizesafe(new float3(random.NextFloat(-1f, 1f), 0.0f,
                            random.NextFloat(-1f, 1f))),
                        Speed = 4.3f - asteroidSize
                    });

                beginCommandBuffer.SetComponent(entityInQueryIndex, newAsteroid,
                    new ConstantRotationData()
                    {
                        Speed = 4.3f - asteroidSize,
                        Angle = new float3(random.NextFloat(0f, 1f), 0f, random.NextFloat(0f, 1f))
                    });
            }

            endCommandBuffer.DestroyEntity(entityInQueryIndex, entity);
        }).ScheduleParallel();

        _endSimulationCommandBufferSystem.AddJobHandleForProducer(Dependency);
        _beginInitCommandBufferSystem.AddJobHandleForProducer(Dependency);
        
        
        // Local functions
        void SpawnAsteroidAtBorders()
        {
            int asteroidSize = _random.NextInt(2, asteroidPrefabData.MaxSize + 1);

            float xSide = _random.NextBool() ? 1f : -1f;
            float zSide = _random.NextBool() ? 1f : -1f;

            float3 asteroidPosition = new float3(
                _random.NextFloat(0f, 21f) * xSide,
                0.0f,
                _random.NextFloat(0f, 4f) * zSide + (8f * zSide));

            Entity newAsteroid = EntityManager.Instantiate(asteroidPrefabData.Prefab);

            EntityManager.SetComponentData(newAsteroid, new Translation {Value = asteroidPosition});
            EntityManager.SetComponentData(newAsteroid, new AsteroidData() {Size = asteroidSize});
            EntityManager.AddComponent<NonUniformScale>(newAsteroid);
            EntityManager.SetComponentData(newAsteroid,
                new NonUniformScale {Value = new float3(asteroidSize * 0.5f)});
            EntityManager.SetComponentData(newAsteroid,
                new MovementData
                {
                    Direction = math.normalizesafe(new float3(_random.NextFloat(-1f, 1f), 0.0f,
                        _random.NextFloat(-1f, 1f))),
                    Speed = 4.3f - asteroidSize
                });

            EntityManager.SetComponentData(newAsteroid,
                new ConstantRotationData()
                {
                    Speed = 4.3f - asteroidSize,
                    Angle = new float3(_random.NextFloat(0f, 1f), 0f, _random.NextFloat(0f, 1f))
                });
        }
    }
}
