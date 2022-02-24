using Unity.Entities;
using UnityEngine;

public class PlayerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.WithAny<PlayerTag>()
            .ForEach((ref ShootingData shootingData, ref PlayerMovementData playerMovementData,
                ref InputData inputData, ref PlayerBehaviourData playerBehaviourData) =>
            {
                if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Respawning)
                {
                    playerMovementData.InputRotation = 0f;
                    playerMovementData.ThrustersOn = false;
                    shootingData.IsShooting = false;
                    playerBehaviourData.HyperSpaceTravelActivated = false;
                    return;
                }

                bool leftKeyPressed = Input.GetKey(inputData.LeftKey);
                bool rightKeyPressed = Input.GetKey(inputData.RightKey);
                bool shootingKeyPressed = Input.GetKeyDown(inputData.ShootKey);
                bool thrustersKeyPressed = Input.GetKey(inputData.ThrustersKey);
                bool hyperSpaceTravelPressed = Input.GetKeyDown(inputData.HyperSpaceTravelKey);

                float inputRotation = 0f;
                inputRotation = rightKeyPressed ? 1.0f : 0.0f;
                inputRotation += leftKeyPressed ? -1.0f : 0.0f;

                playerMovementData.InputRotation = inputRotation;
                playerMovementData.ThrustersOn = thrustersKeyPressed;

                shootingData.IsShooting = shootingKeyPressed;

                if (hyperSpaceTravelPressed)
                {
                    playerBehaviourData.HyperSpaceTravelActivated = true;
                }
            }).Run();
    }
}
