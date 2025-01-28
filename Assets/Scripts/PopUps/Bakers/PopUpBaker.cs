using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

class PopUpBaker : MonoBehaviour
{
    [SerializeField] public PopUpSetter[] setter;
}

[Serializable]
public class PopUpSetter
{
    public string title;
    [TextArea] public string description;
    public float time;
    public bool pauseGame;

    [SerializeField] public EventsSetter[] events;
}

class PopUpBakerBaker : Baker<PopUpBaker>
{
    public override void Bake(PopUpBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        DynamicBuffer<DialogueSpawner> spawners = AddBuffer<DialogueSpawner>(entity);

        var builder = new BlobBuilder(Allocator.Temp);
        ref PopUpArray array = ref builder.ConstructRoot<PopUpArray>();

        var dialogueDataArray = builder.Allocate(ref array.blobs, authoring.setter.Length);

        for (int i = 0; i < dialogueDataArray.Length; i++)
        {
            ref PopUpData data = ref dialogueDataArray[i];
            builder.AllocateString(ref data.title, authoring.setter[i].title);
            builder.AllocateString(ref data.description, authoring.setter[i].description);
            data.maxTime = authoring.setter[i].time;
            data.time = authoring.setter[i].time;
            data.pauseGame = authoring.setter[i].pauseGame;

            var popUpEvents = builder.Allocate(ref data.events, authoring.setter[i].events.Length);

            int spawnerIndex = 0;

            for (int j = 0; j < popUpEvents.Length; j++)
            {
                ref Events events = ref popUpEvents[j];
                events.eventType = authoring.setter[i].events[j].eventType;
                events.cameraShakeIndex = -1;
                events.levelIndex = -1;
                switch (authoring.setter[i].events[j].eventType)
                {
                    case EventType.SPAWNENEMIES:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.setter[i].events[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        events.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    case EventType.ActivateEntities:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.setter[i].events[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        events.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    case EventType.DeactivateEntities:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.setter[i].events[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        events.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    case EventType.ShakeCamera:
                        events.cameraShakeIndex = authoring.setter[i].events[j].shakeIndex;
                        break;
                    case EventType.CHANGELEVEL:
                        events.levelIndex = authoring.setter[i].events[j].levelIndex;
                        break;
                    case EventType.CHOICE:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.setter[i].events[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        events.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    case EventType.POPUP:
                        spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.setter[i].events[j].spawnerGameObject, TransformUsageFlags.Dynamic) });
                        events.entityID = spawnerIndex;
                        spawnerIndex++;
                        break;
                    default:
                        events.entityID = -1;
                        break;
                }
            }
        }

        var blobReference = builder.CreateBlobAssetReference<PopUpArray>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<PopUpArray>(ref blobReference, out var hash);

        AddComponent(entity, new PopUp
        {
            blob = blobReference
        });
    }
}
