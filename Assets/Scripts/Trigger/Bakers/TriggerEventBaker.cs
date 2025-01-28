using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;

class TriggerEventBaker : MonoBehaviour
{
    [SerializeField] public EventsSetter[] events;
}

class TriggerEventBakerBaker : Baker<TriggerEventBaker>
{
    public override void Bake(TriggerEventBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        DynamicBuffer<DialogueSpawner> spawners = AddBuffer<DialogueSpawner>(entity);

        var builder = new BlobBuilder(Allocator.Temp);
        ref TriggerEventsArray array = ref builder.ConstructRoot<TriggerEventsArray>();

        var blobEvents = builder.Allocate(ref array.blobs, authoring.events.Length);

        int spawnerIndex = 0;

        for(int i = 0; i < blobEvents.Length; i++)
        {
            ref Events events = ref blobEvents[i];
            events.eventType = authoring.events[i].eventType;
            events.cameraShakeIndex = -1;
            events.levelIndex = -1;
            switch (authoring.events[i].eventType)
            {
                case EventType.SPAWNENEMIES:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ActivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.DeactivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ShakeCamera:
                    events.cameraShakeIndex = authoring.events[i].shakeIndex;
                    break;
                case EventType.CHANGELEVEL:
                    events.levelIndex = authoring.events[i].levelIndex;
                    break;
                case EventType.CHOICE:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.POPUP:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                default:
                    events.entityID = -1;
                    break;
            }
        }


        var blobReference = builder.CreateBlobAssetReference<TriggerEventsArray>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<TriggerEventsArray>(ref blobReference, out var hash);

        AddComponent(entity, new TriggerEvents
        {
            blob = blobReference
        });
    }
}
