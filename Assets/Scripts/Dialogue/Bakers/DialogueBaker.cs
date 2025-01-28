using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class DialogueBaker : MonoBehaviour
{
    public DialogueSetterArrays[] dialogues;
    public int curIndex;
}

[Serializable]
public class DialogueSetterArrays
{
    public DialogueSetterData[] dialogues;
    public int curIndex;

    public string[] requiredDialogues;
    public string dialogueKey;
}

[Serializable]
public class DialogueSetterData
{
    [TextArea] public string dialogue;
    public int index;

    public DialoguePos pos;

    public float minTime = 2f;

    public Sprite leftSprite;
    public Sprite rightSprite;

    [SerializeField] public EventsSetter[] events;
}

[Serializable]
public class EventsSetter
{
    public EventType eventType = EventType.NONE;
    public GameObject spawnerGameObject;
    public int shakeIndex;
    public int levelIndex;
}

class DialogueBakerBaker : Baker<DialogueBaker>
{
    public override void Bake(DialogueBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var builder = new BlobBuilder(Allocator.Temp);
        ref Dialogues dialoguePool = ref builder.ConstructRoot<Dialogues>();

        dialoguePool.curIndex = 0;

        var dialogueDataArray = builder.Allocate(ref dialoguePool.Value, authoring.dialogues.Length);

        DynamicBuffer<DialogueSpawner> spawners = AddBuffer<DialogueSpawner>(entity);

        int spawnerIndex = 0;

        for (int i = 0; i < dialogueDataArray.Length; i++)
        {
            ref DialogueArray data = ref dialogueDataArray[i];

            data.curIndex = 0;
            builder.AllocateString(ref data.key, authoring.dialogues[i].dialogueKey);
            var requiredDialogueData = builder.Allocate(ref data.requiredDialogues, authoring.dialogues[i].requiredDialogues.Length);
            for (int j = 0; j < requiredDialogueData.Length; j++)
            {
                builder.AllocateString(ref requiredDialogueData[j], authoring.dialogues[i].requiredDialogues[j]);
            }

            var dialogueData = builder.Allocate(ref data.dialogues, authoring.dialogues[i].dialogues.Length);
            for (int j = 0; j < dialogueData.Length; j++)
            {
                ref Dialogue dialogue = ref dialogueData[j];
                dialogue.index = j;
                dialogue.minTime = authoring.dialogues[i].dialogues[j].minTime;
                dialogue.time = 0;
                dialogue.pos = authoring.dialogues[i].dialogues[j].pos;

                var eventsData = builder.Allocate(ref dialogue.OnEndEvents, authoring.dialogues[i].dialogues[j].events.Length);
                for (int k = 0; k < eventsData.Length; k++)
                {
                    ref Events events = ref eventsData[k];
                    events.eventType = authoring.dialogues[i].dialogues[j].events[k].eventType;
                    events.cameraShakeIndex = -1;
                    events.levelIndex = -1;
                    switch (authoring.dialogues[i].dialogues[j].events[k].eventType)
                    {
                        case EventType.SPAWNENEMIES:
                            spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].events[k].spawnerGameObject, TransformUsageFlags.Dynamic) } );
                            events.entityID = spawnerIndex;
                            spawnerIndex++;
                            break;
                        case EventType.ActivateEntities:
                            spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].events[k].spawnerGameObject, TransformUsageFlags.Dynamic) });
                            events.entityID = spawnerIndex;
                            spawnerIndex++;
                            break;
                        case EventType.DeactivateEntities:
                            spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].events[k].spawnerGameObject, TransformUsageFlags.Dynamic) });
                            events.entityID = spawnerIndex;
                            spawnerIndex++;
                            break;
                        case EventType.ShakeCamera:
                            events.cameraShakeIndex = authoring.dialogues[i].dialogues[j].events[k].shakeIndex;
                            break;
                        case EventType.CHANGELEVEL:
                            events.levelIndex = authoring.dialogues[i].dialogues[j].events[k].levelIndex;
                            break;
                        case EventType.CHOICE:
                            spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].events[k].spawnerGameObject, TransformUsageFlags.Dynamic) });
                            events.entityID = spawnerIndex;
                            spawnerIndex++;
                            break;
                        default:
                            events.entityID = -1;
                            break;
                    }
                }

                builder.AllocateString(ref dialogue.leftSpritePath, authoring.dialogues[i].dialogues[j].leftSprite.name);
                builder.AllocateString(ref dialogue.rightSpritePath, authoring.dialogues[i].dialogues[j].rightSprite.name);
                builder.AllocateString(ref dialogue.dialogue, authoring.dialogues[i].dialogues[j].dialogue);
            }
        }


        var blobReference = builder.CreateBlobAssetReference<Dialogues>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<Dialogues>(ref blobReference, out var hash);

        AddComponent(entity, new DialogueData
        {
            Blob = blobReference
        });
    }
}
