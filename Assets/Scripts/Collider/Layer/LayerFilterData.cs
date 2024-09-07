using Unity.Entities;
using Unity.Physics;
using UnityEngine;

public struct LayerFilterData : IComponentData
{
    public CollisionFilter Value;
}
