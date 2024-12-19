using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using Unity.Transforms;
using UnityEditor;

partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((Health health, MaxHealth maxHealth, Entity entity) in SystemAPI.Query<Health, MaxHealth>().WithNone<Dead>().WithEntityAccess())
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

        foreach((Dead dead, DynamicBuffer<Child> children, Entity entity) in SystemAPI.Query<Dead, DynamicBuffer<Child>>().WithNone<DisableEntireEntity>().WithEntityAccess())
        {
            ComponentLookup<MaterialMeshInfo> meshLookup = SystemAPI.GetComponentLookup<MaterialMeshInfo>();

            ecb.AddComponent<DestroyEntity>(entity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
