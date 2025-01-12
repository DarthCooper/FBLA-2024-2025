using Unity.Collections;
using Unity.Entities;

public struct QuestComponents : IComponentData
{
    public BlobAssetReference<Quests> Quests;
}

public struct Quests : IComponentData
{
    public BlobArray<Quest> Blobs;
    public int index;
}

public struct WinConditionElementData : IBufferElementData
{
    public int QuestID;

    public bool requireAll;

    public float maxTime;
    public float curTime;

    public int neededKills;
    public int kills;

    public int neededWaves;
    public int waves;

    public Entity triggerEntity;

    public Entity interactEntity;

    public int killsAdvance;
    public int wavesAdvance;
    public int timeAdvance;
    public int interactAdvance;
}

public struct Quest
{
    public BlobString QuestVisual;
    public BlobString QuestName;
    public int QuestId;
    public bool completed;
}

public struct QuestEndEvent : IBufferElementData
{
    public int QuestID;
    public EventType EventType;
    public Entity spawner;
    public int cameraIndex;
    public int levelIndex;
}

public struct QuestTargetEntity : IComponentData
{
    public Entity Value;
}
