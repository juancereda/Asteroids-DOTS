using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class BattlefieldLimitsSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float _xLimit = 22.5f;
        float _zLimit = 12.5f;
        
        Entities
            .WithAny<MovementData>()
            .ForEach((ref Translation translation) =>
        {
            if (translation.Value.x > _xLimit)
            {
                float3 newTranslation = new float3(-_xLimit, translation.Value.y, translation.Value.z);
                translation.Value = newTranslation;
            }

            if (translation.Value.x < -_xLimit)
            {
                float3 newTranslation = new float3(_xLimit, translation.Value.y, translation.Value.z);
                translation.Value = newTranslation;
            }

            if (translation.Value.z > _zLimit)
            {
                float3 newTranslation = new float3(translation.Value.x, translation.Value.y, -_zLimit);
                translation.Value = newTranslation;
            }

            if (translation.Value.z < -_zLimit)
            {
                float3 newTranslation = new float3(translation.Value.x, translation.Value.y, _zLimit);
                translation.Value = newTranslation;
            }
        }).ScheduleParallel();
    }
}
