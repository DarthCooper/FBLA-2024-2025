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
    public bool requireAll = true;

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

    [Header("QuestAdvances - Should be 1 unless a choice is being made")]
    public int[] advances;

    [Header("On End Event")]
    public EventType questEndEventType;
    public GameObject spawner;
    public int cameraIndex;
    public int levelIndex;
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
        DynamicBuffer<QuestEndEvent> endEvent = AddBuffer<QuestEndEvent>(entity);

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
                requireAll = authoring.quests[i].requireAll,
                maxTime = authoring.quests[i].maxTime,
                curTime = authoring.quests[i].maxTime,
                neededKills = authoring.quests[i].maxKills,
                neededWaves = authoring.quests[i].maxWaves,
                triggerEntity = GetEntity(authoring.quests[i].winTrigger, TransformUsageFlags.Dynamic),
                interactEntity = GetEntity(authoring.quests[i].interactEntity, TransformUsageFlags.Dynamic),
                killsAdvance = authoring.quests[i].advances[0],
                wavesAdvance = authoring.quests[i].advances[1],
                timeAdvance = authoring.quests[i].advances[2],
                interactAdvance = authoring.quests[i].advances[3],
            });

            endEvent.Add(new QuestEndEvent
            {
                QuestID = authoring.quests[i].QuestID,
                EventType = authoring.quests[i].questEndEventType,
                spawner = GetEntity(authoring.quests[i].spawner, TransformUsageFlags.Dynamic),
                cameraIndex = authoring.quests[i].cameraIndex,
                levelIndex = authoring.quests[i].levelIndex,
            });
        }

        AddComponent<QuestTargetEntity>(entity);

        var blobReference = builder.CreateBlobAssetReference<Quests>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<Quests>(ref blobReference, out var hash);

        AddComponent(entity, new QuestComponents
        {
            Quests = blobReference
        });
    }
}
