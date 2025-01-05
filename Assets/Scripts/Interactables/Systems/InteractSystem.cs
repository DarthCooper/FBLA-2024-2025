using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

public partial class InteractSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();

        Entities.WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref PlayerInteractInput input, ref LocalToWorld transform) =>
        {
            if(!input.Value) { return; }
            NativeList<Entity> closestEntities = GetClosest<InteractableTag>(transform.Position, 3f);
            Entity closestInteractable = Entity.Null;
            for(int i = 0; i < closestEntities.Length; i++)
            {
                if(EntityManager.HasComponent<PickUp>(closestEntities[i])) { continue; }
                if (EntityManager.HasComponent<Speaking>(closestEntities[i])) { continue; }
                InteractableType interactableType = GetInteractableType(closestEntities[i]);
                switch (interactableType)
                {
                    case InteractableType.PICKUP:
                        ecb.AddComponent<PickUp>(entityInQueryIndex, closestEntities[i]);
                        break;
                    case InteractableType.DIALOGUE:
                        ecb.AddComponent<Speaking>(entityInQueryIndex, closestEntities[i]);
                        ecb.AddComponent(entityInQueryIndex, entity, new PlayerSpeaking
                        {
                            SpeakingTo = closestEntities[i]
                        });
                        break;
                    case InteractableType.TRIGGER:
                        continue;
                    case InteractableType.NONE:
                        continue;
                }
                break;
            }
        }).Run();

        Entities.WithAll<TriggerInteractableTag>().WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<StatefulTriggerEvent> triggerBuffer, ref InteractableTypeData interactableData) =>
        {
            for (int i = 0; i < triggerBuffer.Length; i++)
            {
                StatefulTriggerEvent triggerEvent = triggerBuffer[i];
                Entity otherEntity = triggerEvent.GetOtherEntity(entity);
                if(!otherEntity.Equals(player)) { continue; }
                if (triggerEvent.State == StatefulEventState.Enter)
                {
                    if(EntityManager.HasComponent<PickUp>(entity)) { continue; }
                    if(EntityManager.HasComponent<Speaking>(entity)) { continue; }
                    InteractableType interactableType = GetInteractableType(entity);
                    switch (interactableType)
                    {
                        case InteractableType.PICKUP:
                            ecb.AddComponent<PickUp>(entityInQueryIndex, entity);
                            break;
                        case InteractableType.DIALOGUE:
                            ecb.AddComponent<Speaking>(entityInQueryIndex, entity);
                            ecb.AddComponent(entityInQueryIndex, player, new PlayerSpeaking
                            {
                                SpeakingTo = entity
                            });
                            break;
                        default:
                            continue;
                    }
                }
            }
        }).Run();
    }

    public NativeList<Entity> GetClosest<T>(float3 origin, float maxDistance = math.INFINITY)
    {
        EntityQuery query = GetEntityQuery(ComponentType.ReadOnly<T>());
        var entities = query.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
        handle.Complete();

        NativeList<Entity> closestEntities = new NativeList<Entity>(Allocator.Temp);

        while(closestEntities.Length < entities.Length)
        {
            float minDist = maxDistance;
            float3 closestPos = float3.zero;
            Entity closestEntity = Entity.Null;
            int closestIndex = 0;
            for (int i = 0; i < entities.Length; i++)
            {
                Entity enemy = entities[i];
                float3 enemyPos = EntityManager.GetComponentData<LocalToWorld>(enemy).Position;

                float dist = Vector3.Distance(origin, enemyPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestPos = enemyPos;
                    closestEntity = enemy;
                    closestIndex = i;
                }
            }

            entities.RemoveAt(closestIndex);
            closestEntities.Add(closestEntity);

        }
        return closestEntities;
    }

    public InteractableType GetInteractableType(Entity interactable)
    {
        if(!EntityManager.HasComponent<InteractableTypeData>(interactable)) { return InteractableType.NONE; }
        return EntityManager.GetComponentData<InteractableTypeData>(interactable).Value;
    }
}
