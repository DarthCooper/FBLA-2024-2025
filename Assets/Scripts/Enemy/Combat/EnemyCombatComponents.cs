using Unity.Entities;

public interface Attacks : IComponentData
{
}

public struct RangedAttack : Attacks
{
    public Entity projectile;

    public float delay;
    public float maxDelay;
    public float speed;

    public float projectileSize;
}

public struct MeleeAttacks: Attacks
{
    public Entity animEntity;
    public Entity pivotEntity;
    public float delay;
    public float maxDelay;
}
