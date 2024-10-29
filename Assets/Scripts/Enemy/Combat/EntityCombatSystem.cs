using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct EntityCombatSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<AtTarget>();
        state.RequireForUpdate<EnemyTag>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((LocalTransform transform, PathFollowTarget target, Entity entity) in SystemAPI.Query<LocalTransform, PathFollowTarget>().WithEntityAccess())
        {
            if(state.EntityManager.HasComponent<RangedAttack>(entity))
            {
                RefRW<RangedAttack> attack = SystemAPI.GetComponentRW<RangedAttack>(entity);
                if(attack.ValueRO.delay <= 0)
                {
                    Debug.Log("Attacking");
                    Entity projectile = ecb.Instantiate(attack.ValueRO.projectile);
                    ecb.SetComponent(projectile, new LocalTransform
                    {
                        Position = transform.Position,
                        Rotation = Quaternion.identity,
                        Scale = 0.15f
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
                        Damage = 10f
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
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
