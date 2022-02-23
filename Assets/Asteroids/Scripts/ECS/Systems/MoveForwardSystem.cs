using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveForwardSystem : SystemBase
{
    protected override void OnUpdate() {

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation position, in MovementData movementData) => {

            float3 normalizedDirection = math.normalizesafe(movementData.Direction);
            position.Value += normalizedDirection * movementData.Speed * deltaTime;

        }).ScheduleParallel();
    }
}
