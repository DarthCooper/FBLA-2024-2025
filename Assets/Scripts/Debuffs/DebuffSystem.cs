using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;

partial struct DebuffSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((RefRW<Stunned> stunTimer, Entity entity) in SystemAPI.Query<RefRW<Stunned>>().WithEntityAccess())
        {
            if(stunTimer.ValueRO.Value <= 0)
            {
                ecb.RemoveComponent<Stunned>(entity);
            }else
            {
                stunTimer.ValueRW.Value -= SystemAPI.Time.DeltaTime;
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
