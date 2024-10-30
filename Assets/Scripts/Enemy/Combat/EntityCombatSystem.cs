using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Rukhanka;
using Unity.Physics;
using Unity.Mathematics;
using UnityEngine.Rendering.VirtualTexturing;
using Unity.Rendering;

partial struct EntityCombatSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((LocalTransform transform, PhysicsVelocity velocity, PathFollowTarget target, Entity entity) in SystemAPI.Query<LocalTransform, PhysicsVelocity, PathFollowTarget>().WithEntityAccess())
        {
            bool atTarget = state.EntityManager.HasComponent<AtTarget>(entity) && state.EntityManager.HasComponent<Hunting>(entity);
            if(state.EntityManager.HasComponent<RangedAttack>(entity))
            {
                RefRW<RangedAttack> attack = SystemAPI.GetComponentRW<RangedAttack>(entity);
                if(attack.ValueRO.delay <= 0)
                {
                    if (!atTarget) { continue; }
                    Entity projectile = ecb.Instantiate(attack.ValueRO.projectile);
                    ecb.SetComponent(projectile, new LocalTransform
                    {
                        Position = transform.Position,
                        Rotation = Quaternion.identity,
                        Scale = attack.ValueRO.projectileSize,
                    });
                    ecb.AddComponent(projectile, new ProjectileDirection
                    {
                        Value = state.EntityManager.GetComponentData<LocalToWorld>(target.Value).Position - transform.Position
                    });
                    ecb.AddComponent(projectile, new ProjectileSpeed
                    {
                        Speed = attack.ValueRO.speed,
                    });
                    ecb.AddComponent(projectile, new ProjectileDamage
                    {
                        Damage = attack.ValueRO.damage,
                    });
                    ecb.AddComponent(projectile, new ProjectileParent
                    {
                        Value = entity
                    });
                    ecb.AddComponent<ProjectileTag>(projectile);
                    attack.ValueRW.delay = attack.ValueRO.maxDelay;
                }else
                {
                    attack.ValueRW.delay -= SystemAPI.Time.DeltaTime;
                }
            }
            if(state.EntityManager.HasComponent<MeleeAttacks>(entity))
            {
                RefRW<MeleeAttacks> attack = SystemAPI.GetComponentRW<MeleeAttacks>(entity);
                DynamicBuffer<AnimationEventComponent> animationEvents = state.EntityManager.GetBuffer<AnimationEventComponent>(attack.ValueRO.animEntity);
                for(int i = 0; i < animationEvents.Length; i++)
                {
                    AnimationEventComponent animationEvent = animationEvents[i];
                    DynamicBuffer<Child> child = state.EntityManager.GetBuffer<Child>(attack.ValueRO.animEntity);
                    Entity sword = child[0].Value;
                    ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();
                    if (animationEvent.stringParamHash.Equals(FixedStringExtensions.CalculateHash32("EnableBlade")))
                    {
                        meshLookup.SetComponentEnabled(sword, true);
                    }else if(animationEvent.stringParamHash.Equals(FixedStringExtensions.CalculateHash32("DisableBlade")))
                    {
                        meshLookup.SetComponentEnabled(sword, false);
                    }

                }
                if (target.Value.Equals(Entity.Null)) { continue; }
                float3 dir = state.EntityManager.GetComponentData<LocalTransform>(target.Value).Position - transform.Position;
                RefRW<LocalTransform> pivot = SystemAPI.GetComponentRW<LocalTransform>(attack.ValueRO.pivotEntity);
                pivot.ValueRW.Rotation = Quaternion.LookRotation(-dir);
                if (attack.ValueRO.delay <= 0)
                {
                    if(!atTarget) { continue; }
                    DynamicBuffer<AnimatorControllerParameterComponent> allParams = state.EntityManager.GetBuffer<AnimatorControllerParameterComponent>(attack.ValueRO.animEntity);
                    var attacking = allParams[0];
                    attacking.SetTrigger();
                    allParams[0] = attacking;

                    attack.ValueRW.delay = attack.ValueRO.maxDelay;
                }
                else
                {
                    attack.ValueRW.delay -= SystemAPI.Time.DeltaTime;
                }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
