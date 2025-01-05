using System;
using Unity.Entities;

public struct DialogueData : IComponentData
{
    public BlobAssetReference<Dialogues> Blob;
}

public struct DialogueSpawner : IBufferElementData
{
    public Entity Spawner;
}

public struct Dialogues : IComponentData
{
    public BlobArray<DialogueArray> Value;
    public int curIndex;
}

[Serializable]
public struct DialogueArray
{
    public BlobArray<Dialogue> dialogues;
    public int curIndex;

    public BlobArray<BlobString> requiredDialogues;
    public BlobString key;
}

[Serializable]
public struct Dialogue
{
    public BlobString dialogue;
    public int index;

    public float minTime;
    public float time;

    public DialoguePos pos;

    public BlobString leftSpritePath;
    public BlobString rightSpritePath;

    public BlobArray<Events> OnEndEvents;
}

public struct IncrementDialogue : IComponentData { }
public struct DisplayedDialogue : IComponentData { }

public struct PlayerSpeaking : IComponentData
{
    public Entity SpeakingTo;
}