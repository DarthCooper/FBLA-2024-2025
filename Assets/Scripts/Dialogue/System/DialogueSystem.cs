using System;
using System.Linq;
using Unity.Entities;
using UnityEngine;

public partial class DialogueSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    public Action<string, DialoguePos, int, string, string> OnTalk;
    public Action OnTalkEnd;

    public Action<float, float> OnDialogueCountdown;

    public Entity playerEntity;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();

        SystemAPI.TryGetSingletonEntity<EventManger>(out Entity eventManger);

        Entities.WithoutBurst().WithAll<Speaking>().ForEach((Entity entity, int entityInQueryIndex, ref DialogueData data) =>
        {
            ref Dialogues dialogues = ref data.Blob.Value;
            if (dialogues.curIndex >= dialogues.Value.Length) {
                ecb.RemoveComponent<Speaking>(entityInQueryIndex, entity);
                ecb.RemoveComponent<InteractableTag>(entityInQueryIndex, entity);
                ecb.RemoveComponent<PlayerSpeaking>(entityInQueryIndex, playerEntity);
                return; 
            }
            ref DialogueArray dialogueArray = ref dialogues.Value[dialogues.curIndex];

            bool allCompleted = true;
            for (int i = 0; i < dialogueArray.requiredDialogues.Length; i++)
            {
                ref BlobString requiredKey = ref dialogueArray.requiredDialogues[i];
                allCompleted = DialogueManager.DialogueComplete(requiredKey.ToString());
            }
            if (!allCompleted) {
                ecb.RemoveComponent<Speaking>(entityInQueryIndex, entity);
                ecb.RemoveComponent<PlayerSpeaking>(entityInQueryIndex, playerEntity);
                return; 
            }

            if (dialogueArray.curIndex >= dialogueArray.dialogues.Length) { return; }
            ref Dialogue curDialogue = ref dialogueArray.dialogues[dialogueArray.curIndex];
            if (EntityManager.HasComponent<IncrementDialogue>(entity))
            {
                if (curDialogue.time >= curDialogue.minTime)
                {
                    dialogueArray.curIndex++;
                    if (dialogueArray.curIndex >= dialogueArray.dialogues.Length)
                    {
                        OnTalkEnd?.Invoke();
                        for (int i = 0; i < curDialogue.OnEndEvents.Length; i++) 
                        {
                            ref Events events = ref curDialogue.OnEndEvents[i];
                            Debug.Log(events.eventType);
                            switch (events.eventType)
                            {
                                case EventType.SPAWNENEMIES:
                                    DynamicBuffer<DialogueSpawner> spawners = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                                    ecb.AddComponent(entityInQueryIndex, eventManger, new SpawnEnemiesEvent
                                    {
                                        spawnEntity = spawners[events.entityID].Spawner
                                    });
                                    break;
                                case EventType.ActivateEntities:
                                    DynamicBuffer<DialogueSpawner> activators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                                    ecb.AddComponent(entityInQueryIndex, eventManger, new ActivateEntitiesEvent
                                    {
                                        ActivateEntityHolder = activators[events.entityID].Spawner
                                    });
                                    break;
                                case EventType.DeactivateEntities:
                                    DynamicBuffer<DialogueSpawner> deactivators = SystemAPI.GetBuffer<DialogueSpawner>(entity);
                                    ecb.AddComponent(entityInQueryIndex, eventManger, new DeActivateEntitiesEvent
                                    {
                                        DeActivateEntityHolder = deactivators[events.entityID].Spawner
                                    });
                                    break;
                                case EventType.ShakeCamera:
                                    ecb.AddComponent(entityInQueryIndex, eventManger, new ShakeCameraEvent
                                    {
                                        index = events.cameraShakeIndex
                                    });
                                    break;
                                case EventType.CHANGELEVEL:
                                    ecb.AddComponent(entityInQueryIndex, eventManger, new EndLevelEvent
                                    {
                                        levelIndex = events.levelIndex
                                    });
                                    break;
                                default:
                                    break;
                            }
                        }
                        ecb.RemoveComponent<Speaking>(entityInQueryIndex, entity);
                        ecb.RemoveComponent<PlayerSpeaking>(entityInQueryIndex, playerEntity);
                        dialogues.curIndex++;
                        DialogueManager.CompleteDialogue(dialogueArray.key.ToString());
                        return;
                    }
                }
                ecb.RemoveComponent<IncrementDialogue>(entityInQueryIndex, entity);
            }


            ref Dialogue dialogue = ref dialogueArray.dialogues[dialogueArray.curIndex];

            dialogue.time += SystemAPI.Time.DeltaTime;
            OnDialogueCountdown?.Invoke(dialogue.time, dialogue.minTime);

            string text = dialogue.dialogue.ToString();
            DialoguePos pos = dialogue.pos;

            string leftSpritePath = dialogue.leftSpritePath.ToString();
            string rightSpritePath = dialogue.rightSpritePath.ToString();

            OnTalk?.Invoke(text, pos, dialogueArray.curIndex, leftSpritePath, rightSpritePath);
        }).Run();

        Entities.WithAll<PlayerSpeaking>().ForEach((Entity entity, int entityInQueryIndex, ref PlayerFire fireInput, ref PlayerJumpInput jumpInput, ref PlayerSpeaking playerSpeaking) =>
        {
            if(playerSpeaking.SpeakingTo.Equals(Entity.Null)) { return; }
            if(!jumpInput.Value && !fireInput.Value) { return; }
            ecb.AddComponent<IncrementDialogue>(entityInQueryIndex, playerSpeaking.SpeakingTo);
        }).Schedule();
    }
}
