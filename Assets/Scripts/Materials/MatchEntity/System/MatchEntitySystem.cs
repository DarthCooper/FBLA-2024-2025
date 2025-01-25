using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public partial class MatchEntitySystem : SystemBase
{
    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
        ComponentLookup<MaterialMeshInfo> meshInfo = SystemAPI.GetComponentLookup<MaterialMeshInfo>();
        Entities.ForEach((Entity entity, int entityInQueryIndex, ref FollowerEntity follower) =>
        {
            if (!meshInfo.HasComponent(follower.Value)) { return; }
            ecb.SetComponent(entityInQueryIndex, entity, meshInfo[follower.Value]);
        }).Schedule();
    }
}
