using System.Numerics;
using Unity.Entities;
using Unity.Mathematics;

public struct GridData : IComponentData
{
    public float3 size;
    public float cellSize;

    public float3 origin;

    public Entity cellChecker;
}
