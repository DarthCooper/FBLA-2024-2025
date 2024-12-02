using Rukhanka;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
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
        NativeList<Entity> hits = new NativeList<Entity>(Allocator.Temp);
        foreach((MeleeAnchor anchor, MeleeAnimHolder anim, MeleeDamage damage, MeleeDirection dir, MeleeSpeed speed, RefRW<MeleeDelay> delay, Parent parent, Entity entity) in SystemAPI.Query<MeleeAnchor, MeleeAnimHolder, MeleeDamage, MeleeDirection, MeleeSpeed, RefRW<MeleeDelay>, Parent>().WithEntityAccess())
        {
            DynamicBuffer<AnimationEventComponent> animationEvents = state.EntityManager.GetBuffer<AnimationEventComponent>(anim.Value);
            bool use = state.EntityManager.HasComponent<Using>(entity);
            #region AnimationEvents
            if(!state.EntityManager.HasBuffer<Child>(anim.Value)) { continue; }
            DynamicBuffer<Child> children = state.EntityManager.GetBuffer<Child>(anim.Value);
            Entity sword = Entity.Null;
            foreach(Child child in children)
            {
                if(state.EntityManager.HasComponent<MaterialMeshInfo>(child.Value))
                {
                    sword = child.Value;
                }
            }
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
            DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer = state.EntityManager.GetBuffer<StatefulTriggerEvent>(anim.Value);
            for(int i = 0; i < triggerEventBuffer.Length; i++)
            {
                var colliderEvent = triggerEventBuffer[i];
                var otherEntity = colliderEvent.GetOtherEntity(anim.Value);
                if (otherEntity.Equals(parent.Value)) { continue; }
                if(hits.Contains(otherEntity)) { continue; }
                if (colliderEvent.State == StatefulEventState.Enter)
                {
                    if (state.EntityManager.HasComponent<Health>(otherEntity))
                    {
                        RefRW<Health> health = SystemAPI.GetComponentRW<Health>(otherEntity);
                        health.ValueRW.Value -= damage.Value;
                    }
                    if (state.EntityManager.HasComponent<PhysicsVelocity>(otherEntity))
                    {
                        ecb.AddComponent<ApplyKnockBack>(otherEntity);
                        ecb.AddComponent(otherEntity, new KnockBackDir
                        {
                            Value = dir.Value,
                        });
                        ecb.AddComponent(otherEntity, new KnockBackStrength
                        {
                            Value = state.EntityManager.GetComponentData<MeleeKnockbackStrength>(entity).Value
                        });
                        ecb.AddComponent(otherEntity, new KnockBackMaxDist
                        {
                            Value = 2f
                        });
                        ecb.AddComponent(otherEntity, new Stunned
                        {
                            Value = 3f
                        });
                    }
                }
            }
            #endregion
            #region StartAttack
            if (delay.ValueRO.Value <= 0)
            {
                if(!use) { continue; }
                ecb.RemoveComponent<Using>(entity);
                if(Mathf.Abs(dir.Value.x) <= 5 && Mathf.Abs(dir.Value.y) <= 5 && Mathf.Abs(dir.Value.z) <= 5)
                {
                    RefRW<PhysicsVelocity> playerVel = SystemAPI.GetComponentRW<PhysicsVelocity>(parent.Value);
                    PhysicsMass playerMass = SystemAPI.GetComponent<PhysicsMass>(parent.Value);
                    playerVel.ValueRW.ApplyLinearImpulse(playerMass, dir.Value * 5);
                    ecb.AddComponent(parent.Value, new Stunned
                    {
                        Value = 0.25f
                    });
                }

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
