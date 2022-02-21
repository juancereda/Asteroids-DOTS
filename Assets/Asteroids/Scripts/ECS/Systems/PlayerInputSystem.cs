using Unity.Entities;
using UnityEngine;

public class PlayerInputSystem : SystemBase {

    protected override void OnUpdate() {
        Entities.ForEach((ref MovementData movementData, in InputData inputData) => {

            bool upKeyPressed = Input.GetKey(inputData.UpKey);
            bool downKeyPressed = Input.GetKey(inputData.DownKey);
            bool leftKeyPressed = Input.GetKey(inputData.LeftKey);
            bool rightKeyPressed = Input.GetKey(inputData.RightKey);

            movementData.Direction.x = rightKeyPressed ? 1.0f : 0.0f;
            movementData.Direction.x -= leftKeyPressed ? 1.0f : 0.0f;
            movementData.Direction.z = upKeyPressed ? 1.0f : 0.0f;
            movementData.Direction.z -= downKeyPressed ? 1.0f : 0.0f;

        }).Run();
    }
}
