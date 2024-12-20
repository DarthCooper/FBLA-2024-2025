using Unity.Entities;

public struct InteractableTag : IComponentData { }

public struct InteractableTypeData : IComponentData
{
    public InteractableType Value;
}

public enum InteractableType
{
    TRIGGER,
    PICKUP,
    DIALOGUE,
    NONE
}

public struct PickUp : IComponentData { }

public struct Speaking : IComponentData { }