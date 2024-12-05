using Unity.Entities;
using UnityEngine;

class EnemyMeleeAuthoring : MonoBehaviour
{
    public GameObject weapon;
}

class EnemyMeleeAuthoringBaker : Baker<EnemyMeleeAuthoring>
{
    public override void Bake(EnemyMeleeAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new Attacks
        {
            weapon = GetEntity(authoring.weapon, TransformUsageFlags.Dynamic)
        });
    }
}
