using System;
using Unity.Entities;
using UnityEngine;

partial class PlayerUISystem : SystemBase
{
    public Action<bool, Entity> toggledEnemy;
    public Action deToggleAll;

    protected override void OnUpdate()
    {
        foreach ((TargetEnemy target, Entity entity) in SystemAPI.Query<TargetEnemy>().WithEntityAccess())
        {
            if(target.Value.Equals(Entity.Null)) { deToggleAll?.Invoke(); continue; }
            toggledEnemy?.Invoke(true, target.Value);
        }
    }
}
