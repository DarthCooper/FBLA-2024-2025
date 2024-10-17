using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowerSystem : SystemBase
{
    EntityCommandBuffer ecb;

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<IsFollowing>();
        RequireForUpdate<PathFollowTarget>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        ecb = new EntityCommandBuffer(Allocator.TempJob);
        foreach ((RefRW<IsFollowing> following, PathFollowTargetDistance targetDistance, PathFollowTarget target, LocalTransform transform, Entity entity) in SystemAPI.Query<RefRW<IsFollowing>, PathFollowTargetDistance, PathFollowTarget, LocalTransform>().WithEntityAccess())
        {
            if(target.Value.Equals(Entity.Null)) { following.ValueRW.Value = false; continue; }
            if(!following.ValueRO.Value) { following.ValueRW.Value = true; }
            SetTarget(transform, target, entity);
            LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(target.Value);
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            if(dist < targetDistance.Value)
            {
                following.ValueRW.Value = false;
            }
            if(!EntityManager.HasComponent<PathFollowRetreatDistances>(entity)) { continue; }
            PathFollowRetreatDistances retreatDistances = EntityManager.GetComponentData<PathFollowRetreatDistances>(entity);
            if(dist <= retreatDistances.Trigger)
            {
                float retreatDistance = UnityEngine.Random.Range(retreatDistances.Min, retreatDistances.Max);
            }
        }
        ecb.Playback(EntityManager);
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
    }

    public void SetTarget(LocalTransform transform, PathFollowTarget target, Entity entity)
    {
        float3 pos = new float3 { x = transform.Position.x, y = transform.Position.z, z = 0 };
        Grid<GridNode> grid = GridSystem.instance.grid;

        if(grid == null) { return; }

        float cellSize = grid.GetCellSize();

        grid.GetXY(pos + new float3(1, 0, 1) * cellSize * .5f, out int startX, out int startY);

        float3 targetPos = EntityManager.GetComponentData<LocalTransform>(target.Value).Position;
        float3 convertedTargetPos = new float3 { x = targetPos.x, y = targetPos.z, z = 0 };

        grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * cellSize * .5f, out int endX, out int endY);

        ValidateGridPosition(ref startX, ref startY, grid);
        ValidateGridPosition(ref endX, ref endY, grid);
        ecb.AddComponent(entity, new PathfindingParams
        {
            startPosition = new int2(startX, startY),
            endPosition = new int2(endX, endY)
        });
    }

    private void ValidateGridPosition(ref int x, ref int y, Grid<GridNode> grid)
    {
        x = math.clamp(x, 0, grid.GetWidth() - 1);
        y = math.clamp(y, 0, grid.GetHeight() - 1);
    }
}
