using Unity.Entities;
using Unity.Mathematics;

public struct MovingObstacleTag : IComponentData { }

public struct StaticObstacleTag : IComponentData { }

[InternalBufferCapacity(50)]
public struct TakenCells : IBufferElementData
{
    public int2 position;
    public bool safe;
    public GridNodeStyle style;
}

public enum GridNodeStyle
{
    Permanant,
    Dynamic
}