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
using System;

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
            bool atTarget = state.EntityManager.HasComponent<AtTarget>(entity);
            if(!atTarget) { continue; }
            if (target.Value.Equals(Entity.Null)) { continue; }
            Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
            if(!target.Value.Equals(player)) { continue; }
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
