using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct FadeInScaleData : IComponentData
{
    public float3 TargetScale;
    public float FadeDuration;
    public float FadeTimer;
}
