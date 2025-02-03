using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;
using UnityEngine;

public partial class HealthSystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    public Action<float3> OnEnemyDeath;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer ecb = _ecbSystem.CreateCommandBuffer();

        foreach ((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithNone<Dead>().WithEntityAccess())
        {
            if(health.Value <= 0)
            {
                CameraManagers.Instance.Impulse(0);
                ecb.AddComponent<Dead>(entity);
            }
            if(health.Value > maxHealth.Value)
            {
                ecb.SetComponent(entity, new Health
                {
                    Value = maxHealth.Value,
                });
            }
        }


        int kills = 0;
        foreach((Dead dead, DynamicBuffer<Child> children, Entity entity) in SystemAPI.Query<Dead, DynamicBuffer<Child>>().WithNone<DisableEntireEntity>().WithEntityAccess())
        {
            ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();

            ecb.AddComponent<DisableEntireEntity>(entity);
            OnEnemyDeath?.Invoke(SystemAPI.GetComponent<LocalTransform>(entity).Position);

            kills += 1;

            PlayerPrefs.SetInt("Kills", PlayerPrefs.GetInt("Kills", 0) + kills);
        }

        QuestSystem questSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<QuestSystem>();
        questSystem.curKills = questSystem.curKills + kills;
    }
}
