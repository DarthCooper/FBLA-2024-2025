using Unity.Entities;
using UnityEngine;

class MagicWeaponAuthoring : MonoBehaviour
{
    public float delay;
    public float damage;

    public float castingTime;

    public CastingType castingType;
}

class MagicWeaponAuthoringBaker : Baker<MagicWeaponAuthoring>
{
    public override void Bake(MagicWeaponAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MagicWeaponDamage
        {
            Value = authoring.damage
        });
        AddComponent(entity, new MagicWeaponDelay
        {
            Delay = authoring.delay,
            MaxDelay = authoring.delay
        });
        AddComponent<MagicWeaponTarget>(entity);

        AddComponent(entity, new CastingTime
        {
            Value = 0,
            MaxValue = authoring.castingTime
        });

        AddComponent(entity, new CastingTypeData
        {
            Value = authoring.castingType
        });
    }
}
