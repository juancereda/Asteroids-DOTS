using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class ConstantRotationSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref Rotation rotation, in AsteroidData asteroidFilter, in MovementData movementData) => {

            if(movementData.RandomRotationEnabled)
            {
                quaternion rotationToAdd = quaternion.EulerXYZ(movementData.RotationAngle * movementData.RotationSpeed * deltaTime);
                quaternion oldRotation = rotation.Value;

                quaternion targetRotation = math.mul(oldRotation, rotationToAdd);
                
                rotation.Value = targetRotation;
            }

        }).ScheduleParallel();
    }
}
