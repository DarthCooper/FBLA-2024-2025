using Unity.Entities;

public struct Attacks : IComponentData
{
    public Entity weapon;
}

public struct RangedAttacks : IComponentData
{
    public Entity weapon;
}

public struct WeaponAttacking : IComponentData { }

