using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;

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
        foreach((KnockBackDir dir, KnockBackStrength force, RefRW<PhysicsVelocity> velocity, PhysicsMass mass, Entity entity) in SystemAPI.Query<KnockBackDir, KnockBackStrength, RefRW<PhysicsVelocity>, PhysicsMass>().WithEntityAccess())
        {
            velocity.ValueRW.ApplyLinearImpulse(mass, dir.Value * force.Value);
            ecb.RemoveComponent<ApplyKnockBack>(entity);
            ecb.RemoveComponent<KnockBackDir>(entity);
            ecb.RemoveComponent<KnockBackStrength>(entity);
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
