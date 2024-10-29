using Unity.Entities;
using Unity.Mathematics;

public struct ProjectileTag : IComponentData { }

public struct ProjectileParent : IComponentData
{
    public Entity Value;
}

public struct ProjectileSpeed : IComponentData
{
    public float Speed;
}

public struct ProjectileDirection : IComponentData
{
    public float3 Value;
}

public struct ProjectileDamage : IComponentData
{
    public float Damage;
}
