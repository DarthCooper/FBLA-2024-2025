using Unity.Entities;
using UnityEngine;

class PathPositionAuthoring : MonoBehaviour
{
    
}

class PathPositionAuthoringBaker : Baker<PathPositionAuthoring>
{
    public override void Bake(PathPositionAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        AddBuffer<PathPosition>(entity);
    }
}
