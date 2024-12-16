using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

partial class Cleanup : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;
    private int _frameCounter;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
        _frameCounter = 0;
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (_frameCounter > 0)
        {
            _frameCounter--;
            return;
        }

        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();

        Entities.WithAll<DisableEntity>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecb.SetEnabled(entityInQueryIndex, entity, false);
        }).ScheduleParallel();

        Entities.WithAll<DisableEntireEntity>().ForEach((Entity entity, int entityInQueryIndex, in DynamicBuffer<Child> children) =>
        {
            ecb.SetEnabled(entityInQueryIndex, entity, false);
            foreach (var child in children)
            {
                if(SystemAPI.HasBuffer<Child>(child.Value))
                {
                    ecb.AddComponent<DisableEntireEntity>(entityInQueryIndex, child.Value);
                }else
                {
                    ecb.AddComponent<DisableEntity>(entityInQueryIndex, child.Value);
                }
            }
        }).ScheduleParallel();

        Entities.WithAll<DestroyEntity>().WithoutBurst().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            DestroyEntity(entity, ecb, entityInQueryIndex);
            ecb.DestroyEntity(entityInQueryIndex, entity);
        }).Run();

        _frameCounter = 1;
    }

    public void DestroyEntity(Entity entity, EntityCommandBuffer.ParallelWriter ecb, int entityInQueryIndex)
    {
        if(SystemAPI.HasBuffer<Child> (entity))
        {
            DynamicBuffer<Child> children = SystemAPI.GetBuffer<Child>(entity);
            foreach (var child in children)
            {
                DestroyEntity(child.Value, ecb, entityInQueryIndex);
            }
        }
        ecb.DestroyEntity(entityInQueryIndex, entity);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
