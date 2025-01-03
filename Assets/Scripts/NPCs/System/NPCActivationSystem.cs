using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct NPCActivationSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((DynamicBuffer<ActivateNPCElement> npcBuffer, Entity entity) in SystemAPI.Query<DynamicBuffer<ActivateNPCElement>>().WithAll<CanActivate>().WithEntityAccess())
        {
            foreach (var npc in npcBuffer)
            {
                if(state.EntityManager.HasComponent<DeActive>(npc.entity))
                {
                    ecb.RemoveComponent<DeActive>(npc.entity);
                }
                ecb.AddComponent<Active>(npc.entity);

                if(!npc.CanAttack && !state.EntityManager.HasComponent<CantAttack>(npc.entity))
                {
                    ecb.AddComponent<CantAttack>(npc.entity);
                }else if(npc.CanAttack && state.EntityManager.HasComponent<CantAttack>(npc.entity))
                {
                    ecb.RemoveComponent<CantAttack>(npc.entity);
                }

                PathFollowTarget target = state.EntityManager.GetComponentData<PathFollowTarget>(npc.entity);
                PathFollowerPreviousTargetDistance targetDist = state.EntityManager.GetComponentData<PathFollowerPreviousTargetDistance>(npc.entity);

                float dist = npc.CanAttack ? targetDist.Value : npc.followDist;

                ChangeTarget(npc.entity, target.Value, npc.target, targetDist.Value, dist, ecb);
            }
            ecb.RemoveComponent<CanActivate>(entity);
        }
        foreach((DynamicBuffer<DeActivateNPCElement> npcBuffer, Entity entity) in SystemAPI.Query<DynamicBuffer<DeActivateNPCElement>>().WithAll<CanDeActivate>().WithEntityAccess())
        {
            foreach (var npc in npcBuffer)
            {
                if(state.EntityManager.HasComponent<Active>(npc.entity))
                {
                    ecb.RemoveComponent<Active>(npc.entity);
                }
                ecb.AddComponent<DeActive>(npc.entity);
            }
        }
        ecb.Playback(state.EntityManager);
    }

    public void ChangeTarget(Entity entity, Entity target, Entity newTarget, float oldDistance, float newDistance, EntityCommandBuffer ecb)
    {
        ecb.SetComponent(entity, new PathFollowerPreviousTarget
        {
            Value = target
        });
        ecb.SetComponent(entity, new PathFollowTarget
        {
            Value = newTarget,
        });

        ecb.SetComponent(entity, new PathFollowTargetDistance
        {
            Value = newDistance,
        });
        ecb.SetComponent(entity, new PathFollowerPreviousTargetDistance
        {
            Value = oldDistance
        });
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
