using Unity.Entities;
using UnityEngine;

class EntityMagicAuthoring : MonoBehaviour
{
    public GameObject weapon;
}

class EntityMagicAuthoringBaker : Baker<EntityMagicAuthoring>
{
    public override void Bake(EntityMagicAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new TargetAttacks
        {
            weapon = GetEntity(authoring.weapon, TransformUsageFlags.Dynamic)
        });
    }
}
