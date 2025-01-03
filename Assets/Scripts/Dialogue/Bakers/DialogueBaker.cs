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

    public EventType eventType = EventType.NONE;
    public GameObject spawnerGameObject;
    public int shakeIndex;
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
                dialogue.eventType = authoring.dialogues[i].dialogues[j].eventType;
                dialogue.cameraShakeIndex = authoring.dialogues[i].dialogues[j].shakeIndex;
                switch (authoring.dialogues[i].dialogues[j].eventType)
                {
                    case EventType.SPAWNENEMIES:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].spawnerGameObject, TransformUsageFlags.Dynamic) } );
                        dialogue.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    case EventType.ActivateEntities:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.dialogues[i].dialogues[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        dialogue.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    default:
                        dialogue.entityID = -1;
                        break;
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
