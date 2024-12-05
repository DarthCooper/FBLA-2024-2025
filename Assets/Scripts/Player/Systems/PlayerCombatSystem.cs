using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
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
                MousePlayerAngle dir = state.EntityManager.GetComponentData<MousePlayerAngle>(entity);
                if(!dir.Value.Equals(float3.zero))
                {
                    transform.ValueRW.Rotation = Quaternion.LookRotation(-dir.Value);
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

                float minDist = float.MaxValue;
                float3 closestPos = new float3(0, 0, 0);
                for (int i = 0; i < entities.Length; i++)
                {
                    Entity enemy = entities[i];
                    float3 enemyPos = state.EntityManager.GetComponentData<LocalToWorld>(enemy).Position;

                    float dist = Vector3.Distance(transform.ValueRO.Position, enemyPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPos = enemyPos;
                    }
                }

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

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
