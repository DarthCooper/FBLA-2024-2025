using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

partial struct PickUpSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        EntityQuery query = state.GetEntityQuery(ComponentType.ReadOnly<PickUp>());
        var entities = query.ToEntityListAsync(Allocator.TempJob, out JobHandle handle);
        handle.Complete();

        foreach (var entity in entities)
        {
            ecb.AddComponent<DestroyEntity>(entity);
        }

        ecb.Playback(state.EntityManager);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
