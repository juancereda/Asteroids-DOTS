using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public class PlayerMovementSystem : SystemBase
{
    private BeginInitializationEntityCommandBufferSystem _endSimulationCommandBufferSystem;

    protected override void OnCreate()
    {
        _endSimulationCommandBufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var endCommandBuffer = _endSimulationCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.ForEach((Entity entity,
            int entityInQueryIndex,
            ref Rotation rotation,
            ref MovementData movementData,
            in PlayerMovementData playerMovementData) =>
        {
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
                    math.normalizesafe(movementData.Direction +
                                       (movementData.Forward * playerMovementData.ThrustersForce * deltaTime));
                endCommandBuffer.RemoveComponent<Disabled>(entityInQueryIndex, playerMovementData.ThrustersMeshEntity);
            }
            else
            {
                endCommandBuffer.AddComponent<Disabled>(entityInQueryIndex, playerMovementData.ThrustersMeshEntity);
            }
            
        }).ScheduleParallel();
        
        _endSimulationCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
