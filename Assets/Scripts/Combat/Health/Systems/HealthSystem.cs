using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

partial struct HealthSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Health>();
    }

    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);
        foreach((Health health, Entity entity) in SystemAPI.Query<Health>().WithNone<Dead>().WithEntityAccess())
        {
            if(health.Value <= 0)
            {
                CameraManagers.Instance.Impulse(0);
                ecb.AddComponent<Dead>(entity);
            }
        }
        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
