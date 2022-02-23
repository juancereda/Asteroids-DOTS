using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class PlayerInputSystem : SystemBase {

    protected override void OnUpdate() {
        Entities.ForEach((ref ShootingData shootingData, ref PlayerMovementData playerMovementData,
            ref InputData inputData) => {
            
            bool leftKeyPressed = Input.GetKey(inputData.LeftKey);
            bool rightKeyPressed = Input.GetKey(inputData.RightKey);
            bool shootingKeyPressed = Input.GetKey(inputData.ShootKey);
            bool thrustersKeyPressed = Input.GetKey(inputData.ThrustersKey);

            float inputRotation = 0f;
            inputRotation = rightKeyPressed ? 1.0f : 0.0f;
            inputRotation += leftKeyPressed ? -1.0f : 0.0f;

            playerMovementData.InputRotation = inputRotation;
            playerMovementData.ThrustersOn = thrustersKeyPressed;

            shootingData.IsShooting = shootingKeyPressed;

        }).Run();
    }
}
