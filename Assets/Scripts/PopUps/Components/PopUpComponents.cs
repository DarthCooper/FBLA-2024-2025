using Unity.Entities;

public struct PopUp : IComponentData
{
    public BlobAssetReference<PopUpArray> blob;
}

public struct PopUpArray : IComponentData
{
    public BlobArray<PopUpData> blobs;
    public int index;
}

public struct PopUpData : IComponentData
{
    public BlobString title;
    public BlobString description;
    public float time;
    public float maxTime;

    public bool pauseGame;

    public BlobArray<Events> events;
}

public struct DisplayPopUp : IComponentData { }
public struct EndPopUp : IComponentData { }