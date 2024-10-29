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
}

public struct MeleeAttacks: Attacks
{

}
