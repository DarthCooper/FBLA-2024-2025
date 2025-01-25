using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public struct Playing : IComponentData
{
    public bool Value;
}

public struct AnimationData: IComponentData
{
    public int index;
    public bool looping;
}

public struct DisabledAnimationData : IComponentData
{
    public Entity entity;
}

public struct MaterialFrame : IBufferElementData
{
    public Entity entity;
    public float time;
    public float maxTime;
}
