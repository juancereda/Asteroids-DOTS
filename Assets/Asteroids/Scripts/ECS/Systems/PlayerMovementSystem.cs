using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerMovementSystem : SystemBase {
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref Rotation rotation, ref MovementData movementData, in PlayerMovementData playerMovementData) => {
            
            if (playerMovementData.InputRotation != 0f)
            {
                quaternion oldRotation = rotation.Value;
                quaternion rotationToAdd = quaternion.EulerXYZ(playerMovementData.RotationSpeed *
                                                               playerMovementData.InputRotation * deltaTime * math.up());
                
                quaternion targetRotation = math.mul(oldRotation, rotationToAdd);

                rotation.Value = targetRotation;
            }
            
            movementData.Forward = math.forward(rotation.Value);
            
            if (playerMovementData.ThrustersOn)
            {
                movementData.Direction = movementData.Forward;
            }

        }).Schedule();
    }
}
