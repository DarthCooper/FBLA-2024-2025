using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEditorInternal;
using UnityEngine;

public partial class QuestSystem : SystemBase
{
    public Action<string, string, bool, int> QuestVisual;
    public Action<int> OnEndQuest;

    public int curKills;
    public int curWaves;

    public NativeList<int> completedQuests = new NativeList<int>(Allocator.Persistent);

    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManger);
        Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref QuestComponents questsData, ref DynamicBuffer<WinConditionElementData> winBuffer, ref DynamicBuffer<QuestEndEvent> eventBuffer) =>
        {
            ref Quests quests = ref questsData.Quests.Value;
            int i = quests.index;
            if(i >= quests.Blobs.Length) { return; }
            ref Quest questData = ref quests.Blobs[i];
            if(questData.completed || completedQuests.Contains(questData.QuestId)) { return; }
            string format = questData.QuestVisual.ToString();
            string questName = questData.QuestName.ToString();
            WinConditionElementData winCondition = default;
            foreach(WinConditionElementData winElementData in winBuffer)
            {
                if(winElementData.QuestID == questData.QuestId)
                {
                    winCondition = winElementData;
                    break;
                }
            }
            Entity targetEntity = EntityManager.GetComponentData<QuestTargetEntity>(entity).Value;
            bool completed = true;
            if (winCondition.neededKills > 0)
            {
                if(curKills < winCondition.neededKills)
                {
                    completed = false;
                }
                string visual = format.Replace("{1}", curKills.ToString()).Replace("{2}", winCondition.neededKills.ToString());
                QuestVisual?.Invoke(questName, visual, completed, winCondition.QuestID);
                if(!targetEntity.Equals(Entity.Null))
                {
                    ecb.SetComponent(entityInQueryIndex, entity, new QuestTargetEntity
                    {
                        Value = Entity.Null,
                    });
                }
            }
            if (winCondition.neededWaves > 0)
            {
                if(curWaves < winCondition.neededWaves)
                {
                    completed = false;
                }
                string visual = format.Replace("{1}", curWaves.ToString()).Replace("{2}", winCondition.neededWaves.ToString());
                QuestVisual?.Invoke(questName, visual, completed, winCondition.QuestID);
                if (!targetEntity.Equals(Entity.Null))
                {
                    ecb.SetComponent(entityInQueryIndex, entity, new QuestTargetEntity
                    {
                        Value = Entity.Null,
                    });
                }
            }
            if (winCondition.maxTime > 0)
            {
                if(winCondition.curTime > 0)
                {
                    winCondition.curTime -= SystemAPI.Time.DeltaTime;
                    completed = false;
                }
                float time = winCondition.curTime;
                int minutes = Mathf.FloorToInt(time / 60F);
                int seconds = Mathf.FloorToInt(time - minutes * 60);

                string niceTime = string.Format("{0:00}:{1:00}", minutes, seconds);
                string visual = format.Replace("{1}", niceTime);
                QuestVisual?.Invoke(questName, visual, completed, winCondition.QuestID);

                if (!targetEntity.Equals(Entity.Null))
                {
                    ecb.SetComponent(entityInQueryIndex, entity, new QuestTargetEntity
                    {
                        Value = Entity.Null,
                    });
                }
            }
            if (!winCondition.triggerEntity.Equals(Entity.Null))
            {
                LocalToWorld triggerTransform = EntityManager.GetComponentData<LocalToWorld>(winCondition.triggerEntity);
                LocalToWorld playerTransform = EntityManager.GetComponentData<LocalToWorld>(player);
                float magnitude = Manager.GetMagnitude(triggerTransform.Position - playerTransform.Position);
                if (magnitude >= 2)
                {
                    completed = false;
                }
                string visual = format.Replace("{1}", magnitude.ToString());
                QuestVisual?.Invoke(questName, visual, completed, winCondition.QuestID);
                if(!targetEntity.Equals(winCondition.triggerEntity))
                {
                    ecb.SetComponent(entityInQueryIndex, entity, new QuestTargetEntity
                    {
                        Value = winCondition.triggerEntity
                    });
                }
            }
            if (!winCondition.interactEntity.Equals(Entity.Null))
            {
                if(!EntityManager.HasComponent<Speaking>(winCondition.interactEntity) && !EntityManager.HasComponent<PickUp>(winCondition.interactEntity)) 
                {
                    completed = false;
                }

                LocalToWorld interactableTransform = EntityManager.GetComponentData<LocalToWorld>(winCondition.interactEntity);
                LocalToWorld playerTransform = EntityManager.GetComponentData<LocalToWorld>(player);
                float magnitude = (int)Manager.GetMagnitude(interactableTransform.Position - playerTransform.Position);
                string visual = format.Replace("{1}", magnitude.ToString());

                QuestVisual?.Invoke(questName, visual, completed, winCondition.QuestID);
                if(!targetEntity.Equals(winCondition.interactEntity))
                {
                    ecb.SetComponent(entityInQueryIndex, entity, new QuestTargetEntity
                    {
                        Value = winCondition.interactEntity
                    });
                }
            }

            if (completed)
            {
                quests.index++;
                questData.completed = true;
                completedQuests.Add(questData.QuestId);
                Debug.Log(quests.index);
                OnEndQuest?.Invoke(questData.QuestId);

                QuestEndEvent questEvent = default;
                foreach (QuestEndEvent eventData in eventBuffer)
                {
                    if (eventData.QuestID == questData.QuestId)
                    {
                        questEvent = eventData;
                        break;
                    }
                }
                switch (questEvent.EventType)
                {
                    case EventType.SPAWNENEMIES:
                        ecb.AddComponent(entityInQueryIndex, eventManger, new SpawnEnemiesEvent
                        {
                            spawnEntity = questEvent.spawner
                        });
                        break;
                    case EventType.ActivateEntities:
                        ecb.AddComponent(entityInQueryIndex, eventManger, new ActivateEntitiesEvent
                        {
                            ActivateEntityHolder = questEvent.spawner
                        });
                        break;
                    case EventType.DeactivateEntities:
                        ecb.AddComponent(entityInQueryIndex, eventManger, new DeActivateEntitiesEvent
                        {
                            DeActivateEntityHolder = questEvent.spawner
                        });
                        break;
                    case EventType.ShakeCamera:
                        ecb.AddComponent(entityInQueryIndex, eventManger, new ShakeCameraEvent
                        {
                            index = questEvent.cameraIndex
                        });
                        break;
                    default:
                        break;
                }

                return;
            }

        }).Run();
    }
}
