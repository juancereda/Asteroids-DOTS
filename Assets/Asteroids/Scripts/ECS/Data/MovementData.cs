using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct MovementData : IComponentData
{
    public bool FaceDirectionEnabled;
    public bool RandomRotationEnabled;
    public float3 Direction;
    public float Speed;
    public float3 RotationAngle;
    public float RotationSpeed;
}
