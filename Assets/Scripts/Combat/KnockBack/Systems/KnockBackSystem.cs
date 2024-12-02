using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

partial struct KnockBackSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<ApplyKnockBack>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((KnockBackDir dir, KnockBackStrength force, KnockBackMaxDist maxDist, RefRW<PhysicsVelocity> velocity, PhysicsMass mass, LocalToWorld transform, Entity entity) in SystemAPI.Query<KnockBackDir, KnockBackStrength, KnockBackMaxDist, RefRW<PhysicsVelocity>, PhysicsMass, LocalToWorld>().WithAll<ApplyKnockBack>().WithEntityAccess())
        {
            if(!state.EntityManager.HasComponent<KnockBackStartPos>(entity))
            {
                ecb.AddComponent(entity, new KnockBackStartPos
                {
                    Value = transform.Position
                });
                continue;
            }
            float3 startPos = state.EntityManager.GetComponentData<KnockBackStartPos>(entity).Value;
            if(Vector3.Distance(startPos, transform.Position) >= maxDist.Value || !state.EntityManager.HasComponent<Stunned>(entity))
            {
                velocity.ValueRW.Linear = float3.zero;
                ecb.RemoveComponent<ApplyKnockBack>(entity);
                ecb.RemoveComponent<KnockBackDir>(entity);
                ecb.RemoveComponent<KnockBackStrength>(entity);
                ecb.RemoveComponent<KnockBackStartPos>(entity);
                ecb.RemoveComponent<KnockBackMaxDist>(entity);
                PreviousLayerFilterData preLayer = state.EntityManager.GetComponentData<PreviousLayerFilterData>(entity);
                ecb.SetComponent(entity, new LayerFilterData
                {
                    Value = preLayer.Value,
                });
                continue;
            }
            ecb.SetComponent(entity, new LayerFilterData
            {
                Value = CollisionFilters.filterSolid
            });
            velocity.ValueRW.Linear = dir.Value * force.Value;
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
