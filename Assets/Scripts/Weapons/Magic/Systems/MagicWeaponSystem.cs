using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

partial struct MagicWeaponSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
        foreach ((MagicWeaponDamage damage, RefRW<MagicWeaponDelay> delay, MagicWeaponTarget target, RefRW<CastingTime> castingTime, Parent parent, Entity entity) in SystemAPI.Query<MagicWeaponDamage, RefRW<MagicWeaponDelay>, MagicWeaponTarget, RefRW<CastingTime>, Parent>().WithEntityAccess())
        {
            bool use = state.EntityManager.HasComponent<Using>(entity);
            bool casting = state.EntityManager.HasComponent<Casting>(parent.Value);
            if (delay.ValueRO.Delay >= delay.ValueRO.MaxDelay && use && !casting)
            {
                delay.ValueRW.Delay = 0;
                ecb.RemoveComponent<Using>(entity);
                ecb.AddComponent<Casting>(parent.Value);
            }
            else if(!casting)
            {
                delay.ValueRW.Delay += SystemAPI.Time.DeltaTime;
                if (use) { ecb.RemoveComponent<Using>(entity); }
            }

            if(casting)
            {
                if(castingTime.ValueRO.Value >= castingTime.ValueRO.MaxValue)
                {
                    ecb.RemoveComponent<Casting>(parent.Value);
                    castingTime.ValueRW.Value = 0;
                }else
                {
                    castingTime.ValueRW.Value += SystemAPI.Time.DeltaTime;
                    if (state.EntityManager.HasComponent<Health>(target.Value))
                    {
                        RefRW<Health> health = SystemAPI.GetComponentRW<Health>(target.Value);
                        health.ValueRW.Value -= damage.Value;
                    }
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
