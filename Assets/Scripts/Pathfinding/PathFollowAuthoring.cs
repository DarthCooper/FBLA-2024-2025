using Unity.Entities;
using UnityEngine;

class PathFollowAuthoring : MonoBehaviour
{
    public int pathIndex = 0;
}

class PathFollowAuthoringBaker : Baker<PathFollowAuthoring>
{
    public override void Bake(PathFollowAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PathFollow
        {
            pathIndex = -1
        });
    }
}
