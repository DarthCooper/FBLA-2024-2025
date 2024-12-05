using Unity.Entities;
using UnityEngine;

class MeleeWeaponAuthoring : MonoBehaviour
{
    public float damage;
    public float speed;
    public GameObject anchor;
    public GameObject animHolder;
    public float delay;
    public float knockback;
    public float knockbackDistance;
    public float stunTime;
    public bool stuns;
    public float dashDist = 5f;
}

class MeleeWeaponAuthoringBaker : Baker<MeleeWeaponAuthoring>
{
    public override void Bake(MeleeWeaponAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new MeleeDamage
        {
            Value = authoring.damage
        });
        AddComponent(entity, new MeleeSpeed
        {
            Value = authoring.speed
        });
        AddComponent(entity, new MeleeAnchor
        {
            Value = GetEntity(authoring.anchor, TransformUsageFlags.Dynamic)
        });
        AddComponent<MeleeDirection>(entity);
        AddComponent(entity, new MeleeAnimHolder
        {
            Value = GetEntity(authoring.animHolder, TransformUsageFlags.Dynamic)
        });
        AddComponent(entity, new MeleeDelay
        {
            Value = 0,
            maxDelay = authoring.delay
        });
        AddComponent(entity, new MeleeKnockbackStrength
        {
            Value = authoring.knockback
        });
        AddBuffer<MeleeHits>(entity);
        AddComponent(entity, new MeleeStunTime
        {
            Value = authoring.stunTime
        });
        AddComponent(entity, new DoesMeleeStuns
        {
            Value = authoring.stuns
        });
        AddComponent(entity, new MeleeKnockbackDistance
        {
            Value = authoring.knockbackDistance
        });
        AddComponent(entity, new MeleeDashDist
        {
            Value = authoring.dashDist
        });
    }
}
