using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

partial struct RangedWeaponsSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((RangedDamage damage, RefRW<RangedDelay> delay, RangedDirection dir, RangedFirepoint firePoint, RangedProjectile projectile, RangedProjectileForce force, RangedProjectileSize scale, Entity entity) in SystemAPI.Query<RangedDamage, RefRW<RangedDelay>, RangedDirection, RangedFirepoint, RangedProjectile, RangedProjectileForce, RangedProjectileSize>().WithEntityAccess())
        {
            bool use = state.EntityManager.HasComponent<Using>(entity);
            if (delay.ValueRO.Value <= 0 && use)
            {
                CameraManagers.Instance.Impulse(1);
                Entity spawnedProjectile = ecb.Instantiate(projectile.Value);
                Entity projectileParent = state.EntityManager.GetComponentData<Parent>(entity).Value;
                float knockback = state.EntityManager.GetComponentData<RangedProjectileKnockback>(entity).Value;
                ecb.SetComponent(spawnedProjectile, new LocalTransform
                {
                    Position = state.EntityManager.GetComponentData<LocalToWorld>(firePoint.Value).Position,
                    Rotation = Quaternion.identity,
                    Scale = scale.Value
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileSpeed
                {
                    Speed = force.Value
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileDamage
                {
                    Damage = damage.Value
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileDirection
                {
                    Value = state.EntityManager.GetComponentData<MousePlayerAngle>(projectileParent).Value
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileParent
                {
                    Value = projectileParent
                });
                ecb.AddComponent(spawnedProjectile, new RangedProjectileKnockback
                {
                    Value = knockback
                });
                ecb.AddComponent<ProjectileTag>(spawnedProjectile);
                ecb.RemoveComponent<Using>(entity);
                delay.ValueRW.Value = delay.ValueRO.MaxValue;
            }else
            {
                delay.ValueRW.Value -= SystemAPI.Time.DeltaTime;
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
