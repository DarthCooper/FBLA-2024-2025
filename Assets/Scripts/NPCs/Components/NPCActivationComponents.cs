using Unity.Entities;

public struct ActivateNPCElement : IBufferElementData
{
    public Entity entity;
    public Entity target;

    public bool CanAttack;

    public float followDist;
}

public struct CantAttack : IComponentData { }

public struct CanActivate : IComponentData { }
