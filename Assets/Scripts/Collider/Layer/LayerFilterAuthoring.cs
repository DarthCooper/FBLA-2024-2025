using Unity.Entities;
using UnityEngine;

class LayerFilterAuthoring : MonoBehaviour
{
    public CollisionLayer layer;
}

class LayerFilterAuthoringBaker : Baker<LayerFilterAuthoring>
{
    public override void Bake(LayerFilterAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(entity, new LayerFilterData
        {
            Value = CollisionFilters.getCollisionFilter(authoring.layer)
        });
    }
}
