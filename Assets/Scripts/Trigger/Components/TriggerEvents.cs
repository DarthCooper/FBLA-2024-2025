using Unity.Entities;

public struct TriggerEvents : IComponentData
{
    public BlobAssetReference<TriggerEventsArray> blob;
}

public struct TriggerEventsArray : IComponentData
{
    public BlobArray<Events> blobs;
}
