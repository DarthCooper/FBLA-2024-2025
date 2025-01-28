using Unity.Entities;

public struct EventManger : IComponentData { }

public struct Events : IComponentData
{
    public EventType eventType;
    public int entityID;

    public int cameraShakeIndex;

    public int levelIndex;
}

public struct SpawnEnemiesEvent : IComponentData
{
    public Entity spawnEntity;
}

public struct EndLevelEvent : IComponentData 
{
    public int levelIndex;
}

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

public struct ChoiceEvent : IComponentData
{
    public Entity entity;
}

public enum EventType
{
    SPAWNENEMIES,
    ENDLEVEL,
    ActivateEntities,
    NONE,
    ShakeCamera,
    DeactivateEntities,
    CHANGELEVEL,
    CHOICE,
}
