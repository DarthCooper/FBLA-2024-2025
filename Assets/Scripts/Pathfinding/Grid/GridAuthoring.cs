using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

class GridAuthoring : MonoBehaviour
{
    public float3 size;
    public float cellSize;

    public float3 origin;

    public GameObject cellChecker;
}

class GridAuthoringBaker : Baker<GridAuthoring>
{
    public override void Bake(GridAuthoring authoring)
    {
        Entity enitity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent(enitity, new GridData
        {
            size = authoring.size,
            cellSize = authoring.cellSize,
            origin = authoring.origin,
            cellChecker = GetEntity(authoring.cellChecker, TransformUsageFlags.Dynamic)
        });
    }
}
