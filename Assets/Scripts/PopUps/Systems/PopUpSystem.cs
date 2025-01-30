using System;
using Unity.Entities;
using UnityEngine;

public partial class PopUpSystem : SystemBase
{
    public Action<string, string, float, float, bool> OnDisplayPopUp;
    public Action ClosePopUp;

    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _ecbSystem.CreateCommandBuffer();

        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManger);

        Entities.WithoutBurst().WithStructuralChanges().WithAll<DisplayPopUp>().ForEach((Entity entity, ref PopUp popUp) =>
        {
            ref PopUpArray array = ref popUp.blob.Value;
            int i = array.index;

            if(i >= array.blobs.Length) { return; }
            ref PopUpData data = ref array.blobs[i];

            OnDisplayPopUp?.Invoke(data.title.ToString(), data.description.ToString(), data.time, data.maxTime, data.pauseGame);

            data.time -= SystemAPI.Time.fixedDeltaTime;

            if(EntityManager.HasComponent<EndPopUp>(entity))
            {
                if(data.time > 0)
                {
                    EntityManager.RemoveComponent<EndPopUp>(entity);
                    return;
                }
                ClosePopUp?.Invoke();
                array.index++;

                for (int j = 0; j < data.events.Length; j++)
                {
                    ref Events events = ref data.events[j];
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
                EntityManager.RemoveComponent<DisplayPopUp>(entity);
                EntityManager.RemoveComponent<EndPopUp>(entity);
            }
        }).Run();

        bool popUpsExist = SystemAPI.TryGetSingletonEntity<PopUp>(out Entity popUp);
        Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref PlayerFire fireInput, ref PlayerJumpInput jumpInput) =>
        {
            if (!jumpInput.Value && !fireInput.Value || !popUpsExist) { return; }
            if (!EntityManager.HasComponent<DisplayPopUp>(popUp)) { return; }
            ecb.AddComponent<EndPopUp>(popUp);
        }).Run();
    }
}
