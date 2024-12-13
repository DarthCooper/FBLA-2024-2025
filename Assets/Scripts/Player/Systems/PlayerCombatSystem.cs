using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct PlayerCombatSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((RefRW<LocalTransform> transform, PlayerFire fire, PlayerAiming aiming, PlayerMeleeWeapon melee, PlayerRangedWeapon ranged, Entity entity) in SystemAPI.Query<RefRW<LocalTransform>, PlayerFire, PlayerAiming, PlayerMeleeWeapon, PlayerRangedWeapon>().WithEntityAccess())
        {
            if (state.EntityManager.HasChunkComponent<Stunned>(entity)) { continue; }
            if (aiming.value)
            {
                MouseWorldPos mousePos = state.EntityManager.GetComponentData<MouseWorldPos>(entity);

                transform.ValueRW.Rotation = Quaternion.identity;
                EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
                var entities = query.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
                handle.Complete();

                float minDist = 2f;
                float3 closestPos = float3.zero;
                Entity closestEntity = Entity.Null;
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity enemy = entities[i];
                    float3 enemyPos = state.EntityManager.GetComponentData<LocalToWorld>(enemy).Position;

                    float dist = Vector3.Distance(mousePos.Value, enemyPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPos = enemyPos;
                        closestEntity = enemy;
                    }
                }

                float3 dir = float3.zero;
                if(closestEntity.Equals(Entity.Null))
                {
                    dir = state.EntityManager.GetComponentData<MousePlayerAngle>(entity).Value;
                }else
                {
                    dir = state.EntityManager.GetComponentData<LocalToWorld>(closestEntity).Position - transform.ValueRO.Position;
                }

                ecb.SetComponent(entity, new TargetEnemy
                {
                    Value = closestEntity
                });
                if (!dir.Equals(float3.zero))
                {
                    transform.ValueRW.Rotation = new Quaternion
                    {
                        x = 0,
                        y = Quaternion.LookRotation(-dir).y,
                        z = 0,
                        w = Quaternion.LookRotation(-dir).w
                    };
                }

                if (fire.Value)
                {
                    ecb.AddComponent<Using>(ranged.Value);
                }
            }
            else
            {
                transform.ValueRW.Rotation = Quaternion.identity;
                EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
                var entities = query.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
                handle.Complete();

                float minDist = 5f;
                float3 closestPos = float3.zero;
                Entity closestEntity = Entity.Null;
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity enemy = entities[i];
                    float3 enemyPos = state.EntityManager.GetComponentData<LocalToWorld>(enemy).Position;

                    float dist = Vector3.Distance(transform.ValueRO.Position, enemyPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPos = enemyPos;
                        closestEntity = enemy;
                    }
                }

                ecb.SetComponent(entity, new TargetEnemy
                {
                    Value = closestEntity,
                });

                if(fire.Value)
                {
                    transform.ValueRW.Rotation = Quaternion.identity;
                    if (melee.Value.Equals(Entity.Null)) { continue; }
                    ecb.AddComponent<Using>(melee.Value);
                    ecb.SetComponent(melee.Value, new MeleeDirection
                    {
                        Value = closestPos - transform.ValueRO.Position
                    });
                }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    public Entity Raycast(float3 RayFrom, float3 RayTo)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

        var raycastInput = new RaycastInput
        {
            Start = RayFrom,
            End = RayTo,
            Filter = CollisionFilters.filterPlayerTrigger
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(RayFrom, RayTo, Color.red);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
