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

    public Action<bool, Entity> OnUseWeapon;
    public Action<float, float, Entity> OnAttackDealy;

    public Action<bool, Entity> OnStun;

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

        foreach ((RangedAttacks rangedWeapon, Entity entity) in SystemAPI.Query<RangedAttacks>().WithAll<EnemyTag>().WithEntityAccess())
        {
            if (!rangedWeapon.weapon.Equals(Entity.Null))
            {
                bool use = EntityManager.HasComponent<Using>(rangedWeapon.weapon);
                OnUseWeapon?.Invoke(use, entity);
                RangedDelay delay = EntityManager.GetComponentData<RangedDelay>(rangedWeapon.weapon);
                OnAttackDealy?.Invoke(delay.Value, delay.MaxValue, entity);
            }
        }

        foreach ((Attacks meleeWeapon, Entity entity) in SystemAPI.Query<Attacks>().WithAll<EnemyTag>().WithEntityAccess())
        {
            if (!meleeWeapon.weapon.Equals(Entity.Null))
            {
                bool attacking = EntityManager.HasComponent<AtTarget>(entity);
                OnUseWeapon?.Invoke(attacking, entity);
                MeleeDelay delay = EntityManager.GetComponentData<MeleeDelay>(meleeWeapon.weapon);
                OnAttackDealy?.Invoke(delay.Value, delay.maxDelay, entity);
            }
        }

        foreach ((EnemyTag tag, Entity entity) in SystemAPI.Query<EnemyTag>().WithEntityAccess())
        {
            bool stunned = EntityManager.HasComponent<Stunned>(entity);
            OnStun?.Invoke(stunned, entity);
        }

    }
}
