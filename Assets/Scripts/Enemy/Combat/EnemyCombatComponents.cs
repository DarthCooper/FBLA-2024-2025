using Unity.Entities;

public struct Attacks : IComponentData
{
    public Entity weapon;
}

public struct RangedAttacks : IComponentData
{
    public Entity weapon;
}

public struct RangedAttack : IComponentData
{
    public Entity projectile;

    public float delay;
    public float maxDelay;
    public float speed;

    public float projectileSize;

    public float damage;
}

