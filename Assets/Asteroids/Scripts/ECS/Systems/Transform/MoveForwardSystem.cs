using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class MoveForwardSystem : SystemBase
{
    protected override void OnUpdate() {

        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Translation position, in MovementData movementData) => {
            position.Value += movementData.Direction * movementData.Speed * deltaTime;

        }).ScheduleParallel();
    }
}
