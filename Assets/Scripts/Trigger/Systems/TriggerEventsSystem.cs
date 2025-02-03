using Unity.Entities;
using Unity.Physics.Stateful;
using UnityEngine;

public partial class TriggerEventsSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        SystemAPI.TryGetSingletonEntity<PlayerTag>(out Entity player);
        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManger);

        Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref TriggerEvents data, ref DynamicBuffer<StatefulTriggerEvent> triggerBuffer) =>
        {
            if(player.Equals(Entity.Null)) { return; }

            for (int i = 0; i < triggerBuffer.Length; i++)
            {
                StatefulTriggerEvent triggerEvent = triggerBuffer[i];
                Entity otherEntity = triggerEvent.GetOtherEntity(entity);
                if (!otherEntity.Equals(player)) { continue; }
                if (triggerEvent.State == StatefulEventState.Enter)
                {
                    if (EntityManager.HasComponent<PickUp>(entity)) { continue; }
                    if (EntityManager.HasComponent<Speaking>(entity)) { continue; }

                    ref TriggerEventsArray array = ref data.blob.Value;
                    for (int j = 0; j < array.blobs.Length; j++)
                    {
                        ref Events events = ref array.blobs[j];
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
                            case EventType.CHOICE:
                                DynamicBuffer<DialogueSpawner> choices = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                                ecb.AddComponent(eventManger, new ChoiceEvent
                                {
                                    entity = choices[events.entityID].Spawner
                                });
                                break;
                            case EventType.POPUP:
                                DynamicBuffer<DialogueSpawner> popUps = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                                ecb.AddComponent(eventManger, new PopUpEvent
                                {
                                    entity = popUps[events.entityID].Spawner
                                });
                                break;
                            default:
                                break;
                        }
                    }
                }
                ecb.DestroyEntity(entity);
            }
        }).Run();
    }
}
