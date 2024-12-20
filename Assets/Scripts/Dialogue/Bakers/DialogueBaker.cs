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
