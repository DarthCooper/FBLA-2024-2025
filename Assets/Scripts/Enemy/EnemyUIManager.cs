using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial class EnemyUIManager : SystemBase
{
    public Action<float3, Entity> OnMove;

    public Action<float, float, Entity> OnTakeDamage;

    public Action<Entity> OnDeath;

    protected override void OnUpdate()
    {
        foreach((LocalToWorld transform, Entity entity) in SystemAPI.Query<LocalToWorld>().WithAll<EnemyTag>().WithEntityAccess())
        {
            OnMove?.Invoke(transform.Position, entity);
        }

        foreach ((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithAll<EnemyTag>().WithEntityAccess())
        {
            OnTakeDamage?.Invoke(health.Value, maxHealth.Value, entity);

            if(health.Value <= 0)
            {
                OnDeath?.Invoke(entity);
            }
        }
    }
}
