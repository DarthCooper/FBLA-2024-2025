using Unity.Entities;

public struct AnimationController : IComponentData
{
    public Entity Value;
}

public struct FlipViaDir : IComponentData
{
    public bool Value;
}

public struct AnimationGraphics : IBufferElementData
{
    public Entity Value;
}

public struct VelocityAnimationController : IComponentData { }
