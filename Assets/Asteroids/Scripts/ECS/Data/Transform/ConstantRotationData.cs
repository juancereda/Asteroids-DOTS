using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ConstantRotationData : IComponentData
{
    public float3 Angle;
    public float Speed;
}