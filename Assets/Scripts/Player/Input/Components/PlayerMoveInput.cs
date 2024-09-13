using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PlayerMoveInput : IComponentData
{
    public float2 Value;
}

public struct PlayerMoveSpeed : IComponentData
{
    public float2 Value;
}

public struct PlayerSprintInput : IComponentData
{
    public bool Value;
}

public struct PlayerSprintSpeed : IComponentData
{
    public float2 Value;
}

public struct PlayerJumpInput : IComponentData
{
    public bool Value;
}

public struct PlayerJumpForce : IComponentData
{
    public float Value;
}
