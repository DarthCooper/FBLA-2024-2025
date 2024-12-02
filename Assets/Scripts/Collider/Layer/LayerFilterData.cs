using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct LayerFilterData : IComponentData
{
    public CollisionFilter Value;
}

public struct PreviousLayerFilterData : IComponentData
{
    public CollisionFilter Value;
}