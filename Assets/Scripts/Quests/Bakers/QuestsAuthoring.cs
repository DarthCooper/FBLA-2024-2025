using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class QuestsAuthoring : MonoBehaviour
{
    [SerializeField] public QuestSetterData[] quests;
}

[Serializable]
public class QuestSetterData
{
    public string questVisual;
    public string questName;
    public int QuestID;

    [Header("Only attach the corresponding value to the win Type")]
    public WinConditons winCondition;

    [Header("Time Based")]
    public float maxTime;

    [Header("Kill Based")]
    public int maxKills;

    [Header("Wave Based")]
    public int maxWaves;

    [Header("Pos Based")]
    public GameObject winTrigger;

    [Header("Interact Based")]
    public GameObject interactEntity;
}

class QuestsAuthoringBaker : Baker<QuestsAuthoring>
{
    public override void Bake(QuestsAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        var builder = new BlobBuilder(Allocator.Temp);
        ref Quests questPool = ref builder.ConstructRoot<Quests>();

        questPool.index = 0;

        DynamicBuffer<WinConditionElementData> winConditions = AddBuffer<WinConditionElementData>(entity);

        var questDataArray = builder.Allocate(ref questPool.Blobs, authoring.quests.Length);
        for (int i = 0; i < questDataArray.Length; i++)
        {
            ref Quest questData = ref questDataArray[i];
            builder.AllocateString(ref questData.QuestVisual, authoring.quests[i].questVisual);
            builder.AllocateString(ref questData.QuestName, authoring.quests[i].questName);
            questData.QuestId = authoring.quests[i].QuestID;
            questData.completed = false;

            winConditions.Add(new WinConditionElementData
            {
                QuestID = authoring.quests[i].QuestID,
                maxTime = authoring.quests[i].maxTime,
                curTime = authoring.quests[i].maxTime,
                neededKills = authoring.quests[i].maxKills,
                neededWaves = authoring.quests[i].maxWaves,
                triggerEntity = GetEntity(authoring.quests[i].winTrigger, TransformUsageFlags.Dynamic),
                interactEntity = GetEntity(authoring.quests[i].interactEntity, TransformUsageFlags.Dynamic),
            });
        }

        var blobReference = builder.CreateBlobAssetReference<Quests>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<Quests>(ref blobReference, out var hash);

        AddComponent(entity, new QuestComponents
        {
            Quests = blobReference
        });
    }
}
