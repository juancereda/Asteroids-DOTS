using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerMovementSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _beginInitEntityCommandBufferSystem;

    protected override void OnCreate()
    {
        _beginInitEntityCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var beginCommandBuffer = _beginInitEntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.ForEach((Entity entity,
            int entityInQueryIndex,
            ref Rotation rotation,
            ref MovementData movementData,
            in PlayerMovementData playerMovementData,
            in PlayerBehaviourData playerBehaviourData) =>
        {
            if (playerBehaviourData.Status == PlayerBehaviourData.PlayerStatus.Respawning)
            {
                beginCommandBuffer.AddComponent<Disabled>(entityInQueryIndex, playerMovementData.ThrustersMeshEntity);
                return;
            }

            if (playerMovementData.InputRotation != 0f)
            {
                quaternion oldRotation = rotation.Value;
                quaternion rotationToAdd = quaternion.EulerXYZ(playerMovementData.RotationSpeed *
                                                               playerMovementData.InputRotation * deltaTime *
                                                               math.up());

                quaternion targetRotation = math.mul(oldRotation, rotationToAdd);

                rotation.Value = targetRotation;
            }

            movementData.Forward = math.forward(rotation.Value);

            if (playerMovementData.ThrustersOn)
            {
                movementData.Direction =
                    math.normalizesafe((movementData.Direction * movementData.Speed * playerMovementData.Inertia) +
                                       (movementData.Forward * playerMovementData.ThrustersForce * deltaTime));
                beginCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerMovementData.ThrustersMeshEntity);

                movementData.Speed = math.lerp(movementData.Speed, playerMovementData.MaxSpeed, playerMovementData.AccelerationFactor);
            }
            else
            {
                beginCommandBuffer.AddComponent<Disabled>(entityInQueryIndex, playerMovementData.ThrustersMeshEntity);
            }

            movementData.Speed -= playerMovementData.Drag * deltaTime;
            movementData.Speed = math.clamp(movementData.Speed, 0f, playerMovementData.MaxSpeed);

        }).ScheduleParallel();
        
        _beginInitEntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
