using Unity.Entities;
using Unity.Mathematics;

public struct AsteroidDestroyedData : IComponentData
{
    public float3 Position;
    public int Size;
}
