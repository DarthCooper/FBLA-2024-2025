using Unity.Entities;

public struct EventManger : IComponentData { }

public struct SpawnEnemiesEvent : IComponentData
{
    public Entity spawnEntity;
}

public struct EndLevelEvent : IComponentData { }

public struct ActivateEntitiesEvent : IComponentData
{
    public Entity ActivateEntityHolder;
}

public struct DeActivateEntitiesEvent : IComponentData
{
    public Entity DeActivateEntityHolder;
}

public struct ShakeCameraEvent : IComponentData
{
    public int index;
}

public enum EventType
{
    SPAWNENEMIES,
    ENDLEVEL,
    ActivateEntities,
    NONE,
    ShakeCamera,
    DeactivateEntities
}
