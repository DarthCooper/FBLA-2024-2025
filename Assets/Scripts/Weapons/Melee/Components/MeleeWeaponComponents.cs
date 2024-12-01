using Unity.Entities;
using Unity.Mathematics;

public struct MeleeDamage : IComponentData
{
    public float Value;
}

public struct MeleeSpeed : IComponentData
{
    public float Value;
}

public struct MeleeAnchor : IComponentData
{
    public Entity Value;
}

public struct MeleeDirection : IComponentData
{
    public float3 Value;
}

public struct MeleeAnimHolder : IComponentData
{
    public Entity Value;
}

public struct MeleeDelay : IComponentData
{
    public float Value;
    public float maxDelay;
}

public struct MeleeKnockbackStrength : IComponentData
{
    public float Value;
}
