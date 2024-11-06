using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct PlayerFire : IComponentData
{
    public bool Value;
}

public struct PlayerAiming : IComponentData
{
    public bool value;
}

public struct PlayerMeleeWeapon : IComponentData
{
    public Entity Value;
}

public struct PlayerRangedWeapon : IComponentData
{
    public Entity Value;
}

public struct MousePlayerAngle : IComponentData
{
    public float3 Value;
}

public struct MouseWorldPos : IComponentData
{
    public float3 Value;
}