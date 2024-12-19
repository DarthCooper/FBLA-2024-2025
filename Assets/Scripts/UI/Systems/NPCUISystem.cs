using System;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public partial class NPCUISystem : SystemBase
{
    public Action<float3, Entity> OnMove;

    public Action<float, float, Entity> OnTakeDamage;

    public Action<Entity> OnDeath;

    protected override void OnUpdate()
    {

    }
}
