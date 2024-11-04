using Unity.Entities;

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
