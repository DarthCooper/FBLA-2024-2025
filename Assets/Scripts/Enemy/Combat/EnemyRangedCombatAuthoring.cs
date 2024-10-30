using Unity.Entities;
using UnityEngine;

class EnemyRangedCombatAuthoring : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float speed = 10f;
    public float delay = 4f;
    public float projectileSize = 0.15f;
    public float damage;
}

class EnemyRangedCombatAuthoringBaker : Baker<EnemyRangedCombatAuthoring>
{
    public override void Bake(EnemyRangedCombatAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RangedAttack
        {
            projectile = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
            speed = authoring.speed,
            maxDelay = authoring.delay,
            projectileSize = authoring.projectileSize,
            damage = authoring.damage,
        });
    }
}
