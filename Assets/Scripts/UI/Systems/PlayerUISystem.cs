using System;
using Unity.Entities;
using UnityEngine;

partial class PlayerUISystem : SystemBase
{
    public Action<bool, Entity> toggledEnemy;
    public Action deToggleAll;

    public Action<float, float> showHealth;

    public Action<float, float> showRangedDelay;
    public Action<float, float> showMeleeDelay;

    protected override void OnUpdate()
    {
        foreach ((TargetEnemy target, Entity entity) in SystemAPI.Query<TargetEnemy>().WithEntityAccess())
        {
            if(target.Value.Equals(Entity.Null)) { deToggleAll?.Invoke(); continue; }
            toggledEnemy?.Invoke(true, target.Value);
        }

        foreach ((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithAll<PlayerTag>().WithEntityAccess())
        {
            showHealth?.Invoke(health.Value, maxHealth.Value);
        }

        foreach ((PlayerRangedWeapon rangedWeapon, PlayerMeleeWeapon meleeWeapon, Entity entity) in SystemAPI.Query<PlayerRangedWeapon, PlayerMeleeWeapon>().WithAll<PlayerTag>().WithEntityAccess())
        {
            if(!rangedWeapon.Value.Equals(Entity.Null))
            {
                RangedDelay delay = EntityManager.GetComponentData<RangedDelay>(rangedWeapon.Value);
                showRangedDelay?.Invoke(delay.Value, delay.MaxValue);
            }

            if (!meleeWeapon.Value.Equals(Entity.Null))
            {
                MeleeDelay delay = EntityManager.GetComponentData<MeleeDelay>(meleeWeapon.Value);
                showMeleeDelay?.Invoke(delay.Value, delay.maxDelay);
            }
        }
    }
}
