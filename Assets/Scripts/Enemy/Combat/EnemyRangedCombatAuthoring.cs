using Unity.Entities;
using UnityEngine;

class EnemyRangedCombatAuthoring : MonoBehaviour
{
    public GameObject weapon;
}

class EnemyRangedCombatAuthoringBaker : Baker<EnemyRangedCombatAuthoring>
{
    public override void Bake(EnemyRangedCombatAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new RangedAttacks
        {
            weapon = GetEntity(authoring.weapon, TransformUsageFlags.Dynamic)
        });
    }
}
