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

        NativeList<int> killedEntities = new NativeList<int>(Allocator.TempJob);

        foreach ((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithNone<Dead>().WithEntityAccess())
        {

            if(health.Value <= 0)
            {
                CameraManagers.Instance.Impulse(0);
                ecb.AddComponent<Dead>(entity);
                ecb.RemoveComponent<Health>(entity);
                OnEnemyDeath?.Invoke(SystemAPI.GetComponent<LocalTransform>(entity).Position);
            }
            if (health.Value > maxHealth.Value)
            {
                ecb.SetComponent(entity, new Health
                {
                    Value = maxHealth.Value,
                });
            }
        }

        Entities.WithAll<EnemyTag>().WithStructuralChanges().ForEach((Dead dead, DynamicBuffer<Child> children, Entity entity) =>
        {
            ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();

            if(killedEntities.Contains(entity.Index))
            {
                return;
            }

            killedEntities.Add(entity.Index);

            Debug.Log(killedEntities.Length + "" + entity.Index);

            QuestSystem questSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<QuestSystem>();
            questSystem.curKills++;

            ecb.AddComponent<DisableEntireEntity>(entity);
            EntityManager.RemoveComponent<EnemyTag>(entity);
        }).Run();
    }
}
