using Unity.Entities;

public struct MagicWeaponTarget : IComponentData
{
    public Entity Value;
}

public struct MagicWeaponDamage : IComponentData
{
    public float Value;
}

public struct MagicWeaponDelay : IComponentData
{
    public float Delay;
    public float MaxDelay;
}

public struct CastingTime : IComponentData
{
    public float Value;
    public float MaxValue;
}

public struct CastingTypeData : IComponentData
{
    public CastingType Value;
}

public struct Casting : IComponentData { }

public enum CastingType
{
    HEALING,
    HARMING,
    LEACHING
}
