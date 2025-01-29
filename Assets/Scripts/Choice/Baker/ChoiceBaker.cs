using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using UnityEngine;

class ChoiceBaker : MonoBehaviour
{
    public GameObject questObject;

    public string description;

    public string button1Text;
    public int button1Progress;
    [SerializeField] public EventsSetter[] button1Events;

    public string button2Text;
    public int button2Progress;
    [SerializeField] public EventsSetter[] button2Events;

}

class ChoiceBakerBaker : Baker<ChoiceBaker>
{
    public override void Bake(ChoiceBaker authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.Dynamic);

        DynamicBuffer<DialogueSpawner> spawners = AddBuffer<DialogueSpawner>(entity);

        var builder = new BlobBuilder(Allocator.Temp);

        ref ChoiceData data = ref builder.ConstructRoot<ChoiceData>();
        builder.AllocateString(ref data.description, authoring.description);

        builder.AllocateString(ref data.button1Description, authoring.button1Text);
        data.button1progress = authoring.button1Progress;

        int spawnerIndex = 0;

        for (int i = 0; i < authoring.button1Events.Length; i++)
        {
            ref Events events = ref data.button1Events[i];
            events.eventType = authoring.button1Events[i].eventType;
            events.cameraShakeIndex = -1;
            events.levelIndex = -1;
            switch (authoring.button1Events[i].eventType)
            {
                case EventType.SPAWNENEMIES:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button1Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ActivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button1Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.DeactivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button1Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ShakeCamera:
                    events.cameraShakeIndex = authoring.button1Events[i].shakeIndex;
                    break;
                case EventType.CHANGELEVEL:
                    events.levelIndex = authoring.button1Events[i].levelIndex;
                    break;
                default:
                    events.entityID = -1;
                    break;
            }
        }

        builder.AllocateString(ref data.button2Description, authoring.button2Text);
        data.button2progress = authoring.button2Progress;

        spawnerIndex = 0;

        for (int i = 0; i < authoring.button2Events.Length; i++)
        {
            ref Events events = ref data.button2Events[i];
            events.eventType = authoring.button2Events[i].eventType;
            events.cameraShakeIndex = -1;
            events.levelIndex = -1;
            switch (authoring.button2Events[i].eventType)
            {
                case EventType.SPAWNENEMIES:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button2Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ActivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button2Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.DeactivateEntities:
                    spawners.Add(new DialogueSpawner { Spawner = GetEntity(authoring.button2Events[i].spawnerGameObject, TransformUsageFlags.Dynamic) });
                    events.entityID = spawnerIndex;
                    spawnerIndex++;
                    break;
                case EventType.ShakeCamera:
                    events.cameraShakeIndex = authoring.button2Events[i].shakeIndex;
                    break;
                case EventType.CHANGELEVEL:
                    events.levelIndex = authoring.button2Events[i].levelIndex;
                    break;
                default:
                    events.entityID = -1;
                    break;
            }
        }

        var blobReference = builder.CreateBlobAssetReference<ChoiceData>(Allocator.Persistent);
        builder.Dispose();

        AddBlobAsset<ChoiceData>(ref blobReference, out var hash);

        AddComponent(entity, new Choice
        {
            blob = blobReference
        });

        AddComponent(entity, new QuestEntity
        {
            Value = GetEntity(authoring.questObject, TransformUsageFlags.Dynamic),
        });
    }
}
