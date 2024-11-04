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
        foreach((LocalTransform transform, PlayerFire fire, PlayerAiming aiming, PlayerMeleeWeapon melee, PlayerRangedWeapon ranged, Entity entity) in SystemAPI.Query<LocalTransform, PlayerFire, PlayerAiming, PlayerMeleeWeapon, PlayerRangedWeapon>().WithEntityAccess())
        {
            EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<EnemyTag>());
            var entities = query.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
            handle.Complete();

            float minDist = float.MaxValue;
            float3 closestPos = new float3(0,0,0);
            for(int i = 0; i < entities.Length; i++)
            {
                Entity enemy = entities[i];
                float3 enemyPos = state.EntityManager.GetComponentData<LocalToWorld>(enemy).Position;

                float dist = Vector3.Distance(transform.Position, enemyPos);
                if (dist < minDist)
                {
                    minDist = dist;
                    closestPos = enemyPos;
                }
            }

            if (!fire.Value) { continue; }
            if (!aiming.value)
            {
                if(melee.Value.Equals(Entity.Null)) { continue; }
                ecb.AddComponent<Using>(melee.Value);
                ecb.SetComponent(melee.Value, new MeleeDirection
                {
                    Value = closestPos - transform.Position
                });
            }
            else
            {
                Debug.Log("Ranged");
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
