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

public struct ProjectileKnockbackDistance : IComponentData
{
    public float Value;
}

public struct DoesProjectileStun : IComponentData
{
    public bool Value;
}

public struct ProjectileStunTime : IComponentData
{
    public float Value;
}
