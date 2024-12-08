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
using Unity.Jobs;

partial struct EntityCombatSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<EnemyTag>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((RefRW<LocalTransform> transform, PhysicsVelocity velocity, PathFollowTarget target, Entity entity) in SystemAPI.Query<RefRW<LocalTransform>, PhysicsVelocity, PathFollowTarget>().WithEntityAccess())
        {
            if(state.EntityManager.HasChunkComponent<Stunned>(entity)) { continue; }
            bool atTarget = state.EntityManager.HasComponent<AtTarget>(entity) && state.EntityManager.HasComponent<Hunting>(entity);
            if (target.Value.Equals(Entity.Null)) { continue; }
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            if(!target.Value.Equals(player)) { continue; }
            /*
            if(state.EntityManager.HasComponent<RangedAttack>(entity))
            {
                RefRW<RangedAttack> attack = SystemAPI.GetComponentRW<RangedAttack>(entity);
                if(attack.ValueRO.delay <= 0)
                {
                    if (!atTarget) { continue; }
                    Entity projectile = ecb.Instantiate(attack.ValueRO.projectile);
                    ecb.SetComponent(projectile, new LocalTransform
                    {
                        Position = transform.ValueRO.Position,
                        Rotation = Quaternion.identity,
                        Scale = attack.ValueRO.projectileSize,
                    });
                    ecb.AddComponent(projectile, new ProjectileDirection
                    {
                        Value = state.EntityManager.GetComponentData<LocalToWorld>(target.Value).Position - transform.ValueRO.Position
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
            */
            if(state.EntityManager.HasComponent<Attacks>(entity) && state.EntityManager.HasComponent<LocalToWorld>(target.Value))
            {
                Attacks melee = state.EntityManager.GetComponentData<Attacks>(entity);
                transform.ValueRW.Rotation = Quaternion.identity;
                if (melee.weapon.Equals(Entity.Null)) { continue; }
                ecb.AddComponent<Using>(melee.weapon);
                ecb.SetComponent(melee.weapon, new MeleeDirection
                {
                    Value = state.EntityManager.GetComponentData<LocalToWorld>(target.Value).Position - transform.ValueRO.Position
                });
            }
            if(state.EntityManager.HasComponent<RangedAttacks>(entity) && state.EntityManager.HasComponent<LocalToWorld>(target.Value))
            {
                RangedAttacks range = state.EntityManager.GetComponentData<RangedAttacks>(entity);
                transform.ValueRW.Rotation = Quaternion.identity;
                if (range.weapon.Equals(Entity.Null)) { continue; }
                LocalToWorld targetPos = state.EntityManager.GetComponentData<LocalToWorld>(target.Value);
                float3 dir = targetPos.Position - transform.ValueRO.Position;
                RefRW<LocalTransform> pistolTransform = SystemAPI.GetComponentRW<LocalTransform>(range.weapon);
                pistolTransform.ValueRW.Rotation = Quaternion.LookRotation(-dir);
                ecb.AddComponent<Using>(range.weapon);
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
