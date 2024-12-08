using Unity.Entities;
using UnityEngine;

class RangedWeaponBaker : MonoBehaviour
{
    public GameObject projectile;
    public GameObject FirePoint;
    public float scale;
    public float damage;
    public float force;
    public float delay;
    public float knockback;
    public float knockbackDist;
    public bool stuns;
    public float stunTime;
}

class RangedWeaponBakerBaker : Baker<RangedWeaponBaker>
{
    public override void Bake(RangedWeaponBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RangedProjectile
        {
            Value = GetEntity(authoring.projectile, TransformUsageFlags.Dynamic)
        });
        AddComponent(entity, new RangedFirepoint
        {
            Value = GetEntity(authoring.FirePoint, TransformUsageFlags.Dynamic)
        });
        AddComponent(entity, new RangedDamage
        {
            Value = authoring.damage
        });
        AddComponent(entity, new RangedProjectileForce
        {
            Value = authoring.force
        });
        AddComponent(entity, new RangedDelay
        {
            Value = 0,
            MaxValue = authoring.delay
        });
        AddComponent(entity, new RangedProjectileSize
        {
            Value = authoring.scale
        });
        AddComponent(entity, new RangedProjectileKnockback
        {
            Value = authoring.knockback
        });
        AddComponent<RangedDirection>(entity);
        AddComponent(entity, new RangedKnockBackDist
        {
            Value = authoring.knockbackDist
        });
        AddComponent(entity, new DoesRangedWeaponStun
        {
            Value = authoring.stuns
        });
        AddComponent(entity, new RangeStunTime
        {
            Value = authoring.stunTime
        });
    }
}
