using Unity.Entities;

public struct EventManger : IComponentData { }

public struct SpawnEnemiesEvent : IComponentData
{
    public Entity spawnEntity;
}

public struct EndLevelEvent : IComponentData { }

public enum EventType
{
    SPAWNENEMIES,
    ENDLEVEL,
    NONE
}
