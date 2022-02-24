using Unity.Mathematics;
using Unity.Entities;
using Unity.Transforms;

public class PowerUpSpawnerSystem : SystemBase
{
    private bool _initialized;
    private Random _random;
    private float _spawnCountDown;

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        PowerUpPrefabData powerUpPrefabData = GetSingleton<PowerUpPrefabData>();

        if (!_initialized)
        {
            _spawnCountDown = powerUpPrefabData.TimeToSpawn;
            _initialized = true;
        }

        if (_spawnCountDown <= 0f)
        {
            if (_random.state == 0)
            {
                _random = new Random((uint) Time.ElapsedTime * 10000);
            }
            
            // Spawn PowerUp
            var newPowerUp = EntityManager.Instantiate(
                _random.NextBool() ? powerUpPrefabData.ShieldPrefab : powerUpPrefabData.OneShotPrefab);
            
            EntityManager.SetComponentData(newPowerUp, 
                new Translation{ Value = 
                    new float3(_random.NextFloat(-19f, 19f), 0.0f, _random.NextFloat(-11f, 11f))});
            
            EntityManager.AddComponent<NonUniformScale>(newPowerUp);
            EntityManager.SetComponentData( newPowerUp, new NonUniformScale{ Value = float3.zero });

            _spawnCountDown = powerUpPrefabData.TimeToSpawn;
        }
        else
        {
            _spawnCountDown -= deltaTime;
        }
    }
}
