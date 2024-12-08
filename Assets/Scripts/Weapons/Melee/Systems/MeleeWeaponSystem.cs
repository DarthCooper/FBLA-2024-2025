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
        foreach ((MeleeAnchor anchor, MeleeAnimHolder anim, MeleeDamage damage, MeleeDirection dir, MeleeSpeed speed, RefRW<MeleeDelay> delay, Parent parent, Entity entity) in SystemAPI.Query<MeleeAnchor, MeleeAnimHolder, MeleeDamage, MeleeDirection, MeleeSpeed, RefRW<MeleeDelay>, Parent>().WithEntityAccess())
        {
            DynamicBuffer<MeleeHits> hits = state.EntityManager.GetBuffer<MeleeHits>(entity);
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
                if (animationEvent.stringParamHash == 2123999296)
                {
                    ecb.SetComponent(anim.Value, new LayerFilterData
                    {
                        Value = CollisionFilters.filterMeleeWeaponTrigger
                    });
                    meshLookup.SetComponentEnabled(sword, true);

                    MeleeDashDist dashDist = state.EntityManager.GetComponentData<MeleeDashDist>(entity);
                    if (Mathf.Abs(dir.Value.x) <= dashDist.Value && Mathf.Abs(dir.Value.y) <= dashDist.Value && Mathf.Abs(dir.Value.z) <= dashDist.Value)
                    {
                        RefRW<PhysicsVelocity> playerVel = SystemAPI.GetComponentRW<PhysicsVelocity>(parent.Value);
                        PhysicsMass playerMass = SystemAPI.GetComponent<PhysicsMass>(parent.Value);
                        playerVel.ValueRW.ApplyLinearImpulse(playerMass, dir.Value * 5);
                        ecb.AddComponent(parent.Value, new Stunned
                        {
                            Value = 0.25f
                        });
                    }
                }
                else if (animationEvent.stringParamHash == 4218191658)
                {
                    ecb.SetComponent(anim.Value, new LayerFilterData
                    {
                        Value = CollisionFilters.filterNone
                    });
                    meshLookup.SetComponentEnabled(sword, false);
                    hits.Clear();
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
                if(state.EntityManager.HasComponent<EnemyTag>(parent.Value))
                {
                    if (state.EntityManager.HasComponent<EnemyTag>(otherEntity)) { continue; }
                }
                if (state.EntityManager.HasComponent<PlayerTag>(parent.Value))
                {
                    if (state.EntityManager.HasComponent<PlayerTag>(otherEntity)) { continue; }
                }
                bool alreadyHit = false;
                foreach(MeleeHits hit in hits)
                {
                    if(hit.Value.Equals(otherEntity))
                    {
                        alreadyHit = true;
                    }
                }
                if(alreadyHit) { continue; }
                if (colliderEvent.State == StatefulEventState.Enter)
                {
                    hits.Add(new MeleeHits { Value = otherEntity});
                    if (state.EntityManager.HasComponent<Health>(otherEntity))
                    {
                        RefRW<Health> health = SystemAPI.GetComponentRW<Health>(otherEntity);
                        health.ValueRW.Value -= damage.Value;
                    }
                    if (state.EntityManager.HasComponent<PhysicsVelocity>(otherEntity))
                    {
                        DoesMeleeStuns stuns = state.EntityManager.GetComponentData<DoesMeleeStuns>(entity);
                        MeleeKnockbackDistance knockbackDist = state.EntityManager.GetComponentData<MeleeKnockbackDistance>(entity);
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
                            Value = knockbackDist.Value
                        });
                        if(stuns.Value)
                        {
                            MeleeStunTime stunTime = state.EntityManager.GetComponentData<MeleeStunTime>(entity);
                            ecb.AddComponent(otherEntity, new Stunned
                            {
                                Value = stunTime.Value
                            });
                        }
                    }
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
                if(use) { ecb.RemoveComponent<Using>(entity); }
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
