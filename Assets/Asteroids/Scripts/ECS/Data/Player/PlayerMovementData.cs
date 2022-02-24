using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerMovementData : IComponentData
{
    public float ThrustersForce;
    public float RotationSpeed;
    public float MaxSpeed;
    public float AccelerationFactor;
    public float Inertia;
    public float Drag;
    public float InputRotation;  // -1 counterclockwise, 1 clockwise, 0 no rotation
    public bool ThrustersOn;
    public Entity ThrustersMeshEntity;
}