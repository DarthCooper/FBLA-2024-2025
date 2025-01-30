using System;
using Unity.Entities;
using UnityEngine;

public partial class ChoiceSystem : SystemBase
{
    public Action<string, string, string> OnShowChoice;

    private EntityCommandBufferSystem commandBufferSystem;


    protected override void OnCreate()
    {
        commandBufferSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = commandBufferSystem.CreateCommandBuffer();
        Entities.WithoutBurst().WithAll<MakeChoice>().ForEach((Entity entity, int entityInQueryIndex, ref Choice choice) =>
        {
            ref ChoiceData data = ref choice.blob.Value;

            OnShowChoice?.Invoke(data.description.ToString(), data.button1Description.ToString(), data.button2Description.ToString());

            ecb.RemoveComponent<MakeChoice>(entity);
            ecb.AddComponent<AwaitingChoice>(entity);
        }).Run();


        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManger);
        Entities.WithoutBurst().WithAll<AwaitingChoice>().ForEach((Entity entity, int entityInQueryIndex, ref Choice choice, ref QuestEntity questEntity) =>
        {
            ref ChoiceData data = ref choice.blob.Value;

            if (EntityManager.HasComponent<Button1Pressed>(entity))
            {
                ecb.AddComponent(questEntity.Value, new AdvanceQuest
                {
                    Value = data.button1progress
                });

                for (int i = 0; i < data.button1Events.Length; i++)
                {
                    ref Events events = ref data.button1Events[i];
                    Debug.Log(events.eventType);
                    switch (events.eventType)
                    {
                        case EventType.SPAWNENEMIES:
                            DynamicBuffer<DialogueSpawner> spawners = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new SpawnEnemiesEvent
                            {
                                spawnEntity = spawners[events.entityID].Spawner
                            });
                            break;
                        case EventType.ActivateEntities:
                            DynamicBuffer<DialogueSpawner> activators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new ActivateEntitiesEvent
                            {
                                ActivateEntityHolder = activators[events.entityID].Spawner
                            });
                            break;
                        case EventType.DeactivateEntities:
                            DynamicBuffer<DialogueSpawner> deactivators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new DeActivateEntitiesEvent
                            {
                                DeActivateEntityHolder = deactivators[events.entityID].Spawner
                            });
                            break;
                        case EventType.ShakeCamera:
                            ecb.AddComponent(eventManger, new ShakeCameraEvent
                            {
                                index = events.cameraShakeIndex
                            });
                            break;
                        case EventType.CHANGELEVEL:
                            ecb.AddComponent(eventManger, new EndLevelEvent
                            {
                                levelIndex = events.levelIndex
                            });
                            break;
                        default:
                            break;
                    }
                }
                ecb.RemoveComponent<Button1Pressed>(entity);
            }
            else if(EntityManager.HasComponent<Button2Pressed>(entity))
            {
                ecb.AddComponent(questEntity.Value, new AdvanceQuest
                {
                    Value = data.button2progress
                });

                for (int i = 0; i < data.button1Events.Length; i++)
                {
                    ref Events events = ref data.button1Events[i];
                    Debug.Log(events.eventType);
                    switch (events.eventType)
                    {
                        case EventType.SPAWNENEMIES:
                            DynamicBuffer<DialogueSpawner> spawners = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new SpawnEnemiesEvent
                            {
                                spawnEntity = spawners[events.entityID].Spawner
                            });
                            break;
                        case EventType.ActivateEntities:
                            DynamicBuffer<DialogueSpawner> activators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new ActivateEntitiesEvent
                            {
                                ActivateEntityHolder = activators[events.entityID].Spawner
                            });
                            break;
                        case EventType.DeactivateEntities:
                            DynamicBuffer<DialogueSpawner> deactivators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                            ecb.AddComponent(eventManger, new DeActivateEntitiesEvent
                            {
                                DeActivateEntityHolder = deactivators[events.entityID].Spawner
                            });
                            break;
                        case EventType.ShakeCamera:
                            ecb.AddComponent(eventManger, new ShakeCameraEvent
                            {
                                index = events.cameraShakeIndex
                            });
                            break;
                        case EventType.CHANGELEVEL:
                            ecb.AddComponent(eventManger, new EndLevelEvent
                            {
                                levelIndex = events.levelIndex
                            });
                            break;
                        default:
                            break;
                    }
                }

                ecb.RemoveComponent<Button2Pressed>(entity);
            }
        }).Run();
    }
}
