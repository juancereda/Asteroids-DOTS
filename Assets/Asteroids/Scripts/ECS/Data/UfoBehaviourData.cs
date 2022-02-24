using Unity.Entities;

[GenerateAuthoringComponent]
public struct UfoBehaviourData : IComponentData
{
    public float RespawnDuration;
    public float RespawnTimer;
    public bool IsAlive;
}
