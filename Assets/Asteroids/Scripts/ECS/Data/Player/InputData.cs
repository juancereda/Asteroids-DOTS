using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct InputData : IComponentData
{
    public KeyCode LeftKey;
    public KeyCode RightKey;
    public KeyCode ShootKey;
    public KeyCode ThrustersKey;
}
