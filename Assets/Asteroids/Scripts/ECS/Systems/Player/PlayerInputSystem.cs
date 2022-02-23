using Unity.Entities;
using UnityEngine;

public class PlayerInputSystem : SystemBase {

    protected override void OnUpdate() {
        Entities.ForEach((ref ShootingData shootingData, ref PlayerMovementData playerMovementData,
            ref InputData inputData, in PlayerBehaviourData playerBehaviourData) => {
            
            if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Respawning)
            {
                playerMovementData.InputRotation = 0f;
                playerMovementData.ThrustersOn = false;
                shootingData.IsShooting = false;
                return;
            }
            
            bool leftKeyPressed = Input.GetKey(inputData.LeftKey);
            bool rightKeyPressed = Input.GetKey(inputData.RightKey);
            bool shootingKeyPressed = Input.GetKeyDown(inputData.ShootKey);
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
