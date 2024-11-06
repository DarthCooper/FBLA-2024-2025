using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics.Stateful;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

partial struct MeleeWeaponSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<MeleeSpeed>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((MeleeAnchor anchor, MeleeAnimHolder anim, MeleeDamage damage, MeleeDirection dir, MeleeSpeed speed, RefRW<MeleeDelay> delay, Parent parent, Entity entity) in SystemAPI.Query<MeleeAnchor, MeleeAnimHolder, MeleeDamage, MeleeDirection, MeleeSpeed, RefRW<MeleeDelay>, Parent>().WithEntityAccess())
        {
            DynamicBuffer<AnimationEventComponent> animationEvents = state.EntityManager.GetBuffer<AnimationEventComponent>(anim.Value);
            bool use = state.EntityManager.HasComponent<Using>(entity);
            #region AnimationEvents
            if(!state.EntityManager.HasBuffer<Child>(anim.Value)) { continue; }
            DynamicBuffer<Child> child = state.EntityManager.GetBuffer<Child>(anim.Value);
            Entity sword = child[0].Value;
            for (int i = 0; i < animationEvents.Length; i++)
            {
                AnimationEventComponent animationEvent = animationEvents[i];
                ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();
                if (animationEvent.stringParamHash.Equals(FixedStringExtensions.CalculateHash32("EnableBlade")))
                {
                    meshLookup.SetComponentEnabled(sword, true);
                }
                else if (animationEvent.stringParamHash.Equals(FixedStringExtensions.CalculateHash32("DisableBlade")))
                {
                    meshLookup.SetComponentEnabled(sword, false);
                }
            }
            #endregion
            #region TriggerDetection
            DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer = state.EntityManager.GetBuffer<StatefulTriggerEvent>(sword);
            for(int i = 0; i < triggerEventBuffer.Length; i++)
            {
                var colliderEvent = triggerEventBuffer[i];
                var otherEntity = colliderEvent.GetOtherEntity(entity);

                if (otherEntity.Equals(parent.Value)) { continue; }
                if (state.EntityManager.HasComponent<Health>(otherEntity))
                {
                    RefRW<Health> health = SystemAPI.GetComponentRW<Health>(otherEntity);
                    if (colliderEvent.State == StatefulEventState.Enter)
                    {
                        health.ValueRW.Value -= damage.Value;
                        ecb.DestroyEntity(entity);
                        continue;
                    }

                }

                // exclude other triggers and processed events
                if (colliderEvent.State == StatefulEventState.Enter)
                {
                    ecb.DestroyEntity(entity);
                }
            }
            #endregion
            #region StartAttack
            if (delay.ValueRO.Value <= 0)
            {
                if(!use) { continue; }
                ecb.RemoveComponent<Using>(entity);

                RefRW<LocalTransform> pivot = SystemAPI.GetComponentRW<LocalTransform>(anchor.Value);
                pivot.ValueRW.Rotation = Quaternion.LookRotation(-dir.Value);
                DynamicBuffer<AnimatorControllerParameterComponent> allParams = state.EntityManager.GetBuffer<AnimatorControllerParameterComponent>(anim.Value);
                var attacking = allParams[0];
                attacking.SetTrigger();
                allParams[0] = attacking;

                delay.ValueRW.Value = delay.ValueRO.maxDelay;
            }else
            {
                delay.ValueRW.Value -= SystemAPI.Time.DeltaTime;
            }
            #endregion
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
