using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

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
            if (delay.ValueRO.Value >= delay.ValueRO.MaxValue && use)
            {
                CameraManagers.Instance.Impulse(1);
                Entity spawnedProjectile = ecb.Instantiate(projectile.Value);
                Entity projectileParent = state.EntityManager.GetComponentData<Parent>(entity).Value;
                float knockback = state.EntityManager.GetComponentData<RangedProjectileKnockback>(entity).Value;
                ecb.AddComponent(firePoint.Value, new SpawnMuzzleFlash());
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
                    Value = state.EntityManager.GetComponentData<LocalToWorld>(firePoint.Value).Forward
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileParent
                {
                    Value = projectileParent
                });
                ecb.AddComponent(spawnedProjectile, new RangedProjectileKnockback
                {
                    Value = knockback
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileKnockbackDistance
                {
                    Value = state.EntityManager.GetComponentData<RangedKnockBackDist>(entity).Value
                });
                ecb.AddComponent(spawnedProjectile, new ProjectileStunTime
                {
                    Value = state.EntityManager.GetComponentData<RangeStunTime>(entity).Value
                });
                ecb.AddComponent(spawnedProjectile, new DoesProjectileStun
                {
                    Value = state.EntityManager.GetComponentData<DoesRangedWeaponStun>(entity).Value
                });
                ecb.AddComponent<ProjectileTag>(spawnedProjectile);
                ecb.RemoveComponent<Using>(entity);
                delay.ValueRW.Value = 0;
            }else
            {
                delay.ValueRW.Value += SystemAPI.Time.DeltaTime;
                if(use) { ecb.RemoveComponent<Using>(entity); }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
