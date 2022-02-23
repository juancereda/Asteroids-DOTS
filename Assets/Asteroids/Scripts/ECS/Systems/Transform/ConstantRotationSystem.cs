using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ConstantRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;

        Entities.ForEach((ref Rotation rotation, in ConstantRotationData constantRotationData) =>
        {
            quaternion rotationToAdd =
                quaternion.EulerXYZ(constantRotationData.Angle * constantRotationData.Speed * deltaTime);
            quaternion oldRotation = rotation.Value;

            quaternion targetRotation = math.mul(oldRotation, rotationToAdd);

            rotation.Value = targetRotation;

        }).ScheduleParallel();
    }
}
