using UnityEngine;
using Unity.Entities;

[GenerateAuthoringComponent]
public struct InputData : IComponentData
{
    public KeyCode UpKey;
    public KeyCode DownKey;
    public KeyCode LeftKey;
    public KeyCode RightKey;
    public KeyCode ShootKey;
}
