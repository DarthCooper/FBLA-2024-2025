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

        NativeList<Entity> killedEntities = new NativeList<Entity>(Allocator.Persistent);

        int kills = 0;
        foreach ((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithNone<Dead>().WithEntityAccess())
        {

            if(health.Value <= 0)
            {
                if(killedEntities.Contains(entity)) { continue; }

                kills += 1;
                CameraManagers.Instance.Impulse(0);
                ecb.AddComponent<Dead>(entity);
                ecb.RemoveComponent<Health>(entity);

                killedEntities.Add(entity);
            }
            if(health.Value > maxHealth.Value)
            {
                ecb.SetComponent(entity, new Health
                {
                    Value = maxHealth.Value,
                });
            }
        }


        foreach((Dead dead, DynamicBuffer<Child> children, Entity entity) in SystemAPI.Query<Dead, DynamicBuffer<Child>>().WithNone<DisableEntireEntity>().WithAll<EnemyTag>().WithEntityAccess())
        {
            ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();

            ecb.AddComponent<DisableEntireEntity>(entity);
            ecb.RemoveComponent<EnemyTag>(entity);
            OnEnemyDeath?.Invoke(SystemAPI.GetComponent<LocalTransform>(entity).Position);
        }

        QuestSystem questSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<QuestSystem>();
        questSystem.curKills += kills;
        Debug.Log("Kills: " + questSystem.curKills);
    }
}
