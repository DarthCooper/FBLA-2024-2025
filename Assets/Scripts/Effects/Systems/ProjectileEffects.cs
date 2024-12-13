using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class ProjectileEffects : SystemBase
{
    public Action<float3, Entity> OnBulletMove;
    public Action<float3, Quaternion> OnSpawnBullet;



    protected override void OnUpdate()
    {
        var ecbSystem = World.GetOrCreateSystemManaged<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = ecbSystem.CreateCommandBuffer();
        foreach ((LocalTransform transform, Entity entity) in SystemAPI.Query<LocalTransform>().WithAll<ProjectileTag>().WithEntityAccess())
        {
            OnBulletMove?.Invoke(transform.Position, entity);
        }

        foreach((LocalToWorld transform, Entity entity) in SystemAPI.Query<LocalToWorld>().WithAll<SpawnMuzzleFlash>().WithEntityAccess())
        {
            OnSpawnBullet?.Invoke(transform.Position, transform.Rotation);
            ecb.RemoveComponent<SpawnMuzzleFlash>(entity);
        }
    }
}
