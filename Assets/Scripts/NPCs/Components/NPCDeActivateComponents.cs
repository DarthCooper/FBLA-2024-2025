using Unity.Entities;

public struct DeActivateNPCElement : IBufferElementData
{
    public Entity entity;
}

public struct CanDeActivate : IComponentData { }
