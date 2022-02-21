using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class FaceDirectionSystem : SystemBase {
    protected override void OnUpdate() {

        Entities.ForEach((ref Translation position, ref Rotation rotation, in MovementData movementData) => {

            if(!movementData.Direction.Equals(float3.zero)) {
                quaternion targetRotation = quaternion.LookRotationSafe(movementData.Direction, math.up());
                rotation.Value = math.slerp(rotation.Value, targetRotation, movementData.RotationSpeed);
            }

        }).ScheduleParallel();
    }
}
