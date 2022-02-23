using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class PlayerMovementSystem : SystemBase {
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        
        Entities.ForEach((ref Rotation rotation, ref MovementData movementData, in PlayerMovementData playerMovementData) => {
            
            if (playerMovementData.InputRotation != 0f)
            {
                quaternion rotationToAdd = quaternion.EulerXYZ(playerMovementData.RotationSpeed *
                                                               playerMovementData.InputRotation * deltaTime * math.up());

                quaternion oldRotation = rotation.Value;
                quaternion targetRotation = math.mul(oldRotation, rotationToAdd);

                rotation.Value = targetRotation;
                movementData.Forward = math.forward(rotation.Value);
            }
            
            if (playerMovementData.ThrustersOn)
            {
                movementData.Direction = movementData.Forward;
            }

        }).Schedule();
    }
}
