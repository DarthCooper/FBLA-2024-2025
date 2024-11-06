using Unity.Entities;
using Unity.Mathematics;

public struct RangedDamage : IComponentData
{
    public float Value;
}

public struct RangedDirection : IComponentData
{
    public float3 Value;
}

public struct RangedProjectileForce : IComponentData
{
    public float Value;
}

public struct RangedProjectile : IComponentData
{
    public Entity Value;
}

public struct RangedFirepoint : IComponentData
{
    public Entity Value;
}

public struct RangedProjectileSize : IComponentData
{
    public float Value;
}

public struct RangedProjectileParent : IComponentData
{
    public Entity Value;
}

public struct RangedDelay : IComponentData
{
    public float Value;
    public float MaxValue;
}