using Unity.Entities;

public struct Choice : IComponentData
{
    public BlobAssetReference<ChoiceData> blob;
}

public struct ChoiceData : IComponentData
{
    public BlobString description;

    public BlobString button1Description;
    public int button1progress;
    public BlobArray<Events> button1Events;

    public BlobString button2Description;
    public int button2progress;
    public BlobArray<Events> button2Events;
}

public struct QuestEntity : IComponentData
{
    public Entity Value;
}

public struct MakeChoice : IComponentData { }
partial struct AwaitingChoice : IComponentData { }

public struct Button1Pressed : IComponentData { }

public struct Button2Pressed : IComponentData { }