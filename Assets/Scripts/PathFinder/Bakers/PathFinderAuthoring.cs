using Unity.Entities;
using UnityEngine;

class PathFinderAuthoring : MonoBehaviour
{
    
}

class PathFinderAuthoringBaker : Baker<PathFinderAuthoring>
{
    public override void Bake(PathFinderAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        DynamicBuffer<PathPosition> pathPositions = AddBuffer<PathPosition>(entity);
        AddComponent<PathFinderTag>(entity);
        AddComponent(entity, new PathFollow
        {
            pathIndex = -1
        });
    }
}
