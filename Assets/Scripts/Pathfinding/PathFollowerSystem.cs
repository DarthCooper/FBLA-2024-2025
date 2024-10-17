using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using static UnityEngine.RuleTile.TilingRuleOutput;

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
        foreach ((RefRW<IsFollowing> following, PathFollowTargetDistance targetDistance, PathFollowTarget target, PathFollowerPreviousTarget lastTarget, LocalTransform transform, Entity entity) in SystemAPI.Query<RefRW<IsFollowing>, PathFollowTargetDistance, PathFollowTarget, PathFollowerPreviousTarget, LocalTransform>().WithEntityAccess())
        {
            if(target.Value.Equals(Entity.Null)) { following.ValueRW.Value = false; continue; }
            if(!following.ValueRO.Value) { following.ValueRW.Value = true; }
            SetTarget(transform, target, entity);
            LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(target.Value);
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            if(dist < targetDistance.Value)
            {
                following.ValueRW.Value = false;
                if(EntityManager.HasComponent<Retreating>(entity))
                {
                    ecb.DestroyEntity(target.Value);
                    ecb.RemoveComponent<Retreating>(entity);
                    ChangeTarget(entity, Entity.Null, lastTarget.Value, 1f, EntityManager.GetComponentData<PathFollowerPreviousTargetDistance>(entity).Value);
                }
            }
            CheckRetreating(dist, entity, transform, target);
        }
        ecb.Playback(EntityManager);
    }

    public void CheckRetreating(float dist, Entity entity, LocalTransform transform, PathFollowTarget target)
    {
        if (!EntityManager.HasComponent<PathFollowRetreatDistances>(entity)) { return; }
        PathFollowRetreatDistances retreatDistances = EntityManager.GetComponentData<PathFollowRetreatDistances>(entity);
        if (dist <= retreatDistances.Trigger && !EntityManager.HasComponent<Retreating>(entity))
        {
            Grid<GridNode> grid = GridSystem.instance.grid;
            if (grid == null) { return; }

            float3 targetPos = EntityManager.GetComponentData<LocalTransform>(target.Value).Position;

            float retreatDistance = UnityEngine.Random.Range(retreatDistances.Min, retreatDistances.Max);
            float3 retreatDir = transform.Position - targetPos;
            Entity retreatEntity = EntityManager.CreateEntity();

            float3 targetGoal = Vector3.Normalize(retreatDir) * retreatDistance;

            float3 convertedTargetPos = new float3 { x = targetGoal.x, y = targetGoal.z, z = 0 };

            grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * grid.GetCellSize() * .5f, out int endX, out int endY);
            ValidateGridPosition(ref endX, ref endY, grid);

            ecb.AddComponent(retreatEntity, new LocalTransform
            {
                Position = grid.GetWorldPosition(endX, endY),
                Rotation = Quaternion.identity,
                Scale = 1
            });

            ecb.SetComponent(entity, new PathFollowTargetDistance
            {
                Value = 1
            });

            ChangeTarget(entity, target.Value, retreatEntity, EntityManager.GetComponentData<PathFollowTargetDistance>(entity).Value, 1f);
            ecb.AddComponent<Retreating>(entity);
        }
    }

    [BurstCompile]
    protected override void OnDestroy()
    {
    }

    public void ChangeTarget(Entity entity, Entity target, Entity newTarget, float oldDistance, float newDistance)
    {
        ecb.SetComponent(entity, new PathFollowerPreviousTarget
        {
            Value = target
        });
        ecb.SetComponent(entity, new PathFollowTarget
        {
            Value = newTarget,
        });

        ecb.SetComponent(entity, new PathFollowTargetDistance
        {
            Value = newDistance,
        });
        ecb.SetComponent(entity, new PathFollowerPreviousTargetDistance
        {
            Value = oldDistance,
        });
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
