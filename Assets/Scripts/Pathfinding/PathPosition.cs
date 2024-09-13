using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(50)]
public struct PathPosition : IBufferElementData
{
    public int2 position;
}
