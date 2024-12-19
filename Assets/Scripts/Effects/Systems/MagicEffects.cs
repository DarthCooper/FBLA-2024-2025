using Unity.Entities;
using System;
using Unity.Mathematics;
using Unity.Transforms;

public partial class MagicEffects : SystemBase
{
    public Action<float3, CastingType, Entity> OnCast;
    protected override void OnUpdate()
    {
        foreach ((TargetAttacks attack, Entity entity) in SystemAPI.Query<TargetAttacks>().WithAll<Casting>().WithEntityAccess())
        {
            if (attack.weapon.Equals(Entity.Null)) { continue; }
            MagicWeaponTarget magicWeaponTarget = EntityManager.GetComponentData<MagicWeaponTarget>(attack.weapon);
            if(magicWeaponTarget.Value.Equals(Entity.Null) || !SystemAPI.Exists(magicWeaponTarget.Value)) { continue; }
            if(!EntityManager.HasComponent<LocalToWorld>(magicWeaponTarget.Value)) { continue; }
            CastingTypeData castType = EntityManager.GetComponentData<CastingTypeData>(attack.weapon);
            float3 pos = EntityManager.GetComponentData<LocalToWorld>(magicWeaponTarget.Value).Position;
            OnCast?.Invoke(pos, castType.Value, entity);
        }
    }
}
