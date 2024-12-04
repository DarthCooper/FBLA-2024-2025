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

        Entities.WithAll<DestroyEntity>().ForEach((Entity entity, int entityInQueryIndex) =>
        {
            ecb.SetEnabled(entityInQueryIndex, entity, false);
            if(SystemAPI.HasBuffer<Child>(entity))
            {
                DynamicBuffer<Child> children = SystemAPI.GetBuffer<Child>(entity);
                foreach (var child in children)
                {
                    ecb.AddComponent<DestroyEntity>(entityInQueryIndex, child.Value);
                }
            }

        }).Schedule();

        _frameCounter = 1;
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}
