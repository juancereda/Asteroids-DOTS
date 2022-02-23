using Unity.Entities;

[GenerateAuthoringComponent]
public struct PlayerMovementData : IComponentData
{
    public float ThrustersForce;
    public float RotationSpeed;
    public float InputRotation;  // -1 counterclockwise, 1 clockwise, 0 no rotation
    public bool ThrustersOn;
    public Entity ThrustersMeshEntity;
}