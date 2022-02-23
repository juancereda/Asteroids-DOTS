using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class AsteroidsSpawnerSystem : SystemBase
{
    private bool _spawned = false;
    private Random _random;

    protected override void OnCreate()
    {
        _random = new Random(27);
    }

    protected override void OnUpdate()
    {
        if (!_spawned)
        {
            AsteroidPrefabData asteroidPrefabData = GetSingleton<AsteroidPrefabData>();

            for (int i = 0; i < 10; i++)
            {
                int asteroidSize = _random.NextInt(1, asteroidPrefabData.MaxSize);
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
    }
}
