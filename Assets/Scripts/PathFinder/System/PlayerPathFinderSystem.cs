using System;
using System.Globalization;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public partial class PlayerPathFinderSystem : SystemBase
{
    public Action<float3[]> OnPathFind;
    public Action HidePathFinder;

    private EntityCommandBufferSystem _ecbSystem;

    protected override void OnCreate()
    {
        _ecbSystem = World.GetOrCreateSystemManaged<EntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        Entity player = SystemAPI.GetSingletonEntity<PlayerTag>();
        Entity questTarget = SystemAPI.GetSingleton<QuestTargetEntity>().Value;
        EntityCommandBuffer.ParallelWriter ecb = _ecbSystem.CreateCommandBuffer().AsParallelWriter();
        Entities.WithAll<PathFinderTag>().WithoutBurst().ForEach((Entity entity, int entityInQueryIndex, ref DynamicBuffer<PathPosition> pathPositionBuffer, ref PathFollow pathFollow) =>
        {
            if(questTarget.Equals(Entity.Null)) { HidePathFinder?.Invoke(); return; }
            LocalToWorld playerTransform = EntityManager.GetComponentData<LocalToWorld>(player);
            LocalToWorld targetTransform = EntityManager.GetComponentData<LocalToWorld>(questTarget);

            float3 pos = new float3 { x = playerTransform.Position.x, y = playerTransform.Position.z, z = 0 };
            Grid<GridNode> grid = GridSystem.instance.grid;

            if (grid == null) { return; }

            float cellSize = grid.GetCellSize();

            grid.GetXY(pos + new float3(1, 0, 1) * cellSize * .5f, out int startX, out int startY);

            float3 convertedTargetPos = new float3 { x = targetTransform.Position.x, y = targetTransform.Position.z, z = 0 };

            grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * cellSize * .5f, out int endX, out int endY);

            ValidateGridPosition(ref startX, ref startY, grid);
            ValidateGridPosition(ref endX, ref endY, grid);
            if (EntityManager.HasComponent<PathfindingParams>(entity))
            {
                ecb.SetComponent(entityInQueryIndex, entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }
            else
            {
                ecb.AddComponent(entityInQueryIndex, entity, new PathfindingParams
                {
                    startPosition = new int2(startX, startY),
                    endPosition = new int2(endX, endY)
                });
            }

            if (pathFollow.pathIndex != pathPositionBuffer.Length - 2)
            {
                pathFollow.pathIndex = pathPositionBuffer.Length - 2;
            }
            if (pathFollow.pathIndex <= -1)
            {
                pathFollow.pathIndex = -1;
                return;
            }

            NativeList<float3> convertedPathPositions = new NativeList<float3>(Allocator.Temp);

            if (pathFollow.pathIndex >= 0 && pathFollow.pathIndex < pathPositionBuffer.Length && pathPositionBuffer.Length > 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

                //grid.GetGridObject(pathPosition.x, pathPosition.y).SetIsWalkable(false);
                // enemyPositions.Add(pathPosition);

                float3 targetPosition = new float3(pathPosition.x, 0, pathPosition.y);

                for (int i = pathPositionBuffer.Length - 1; i > 0; i--)
                {
                    Vector3 startPos = new Vector3(pathPositionBuffer[i].position.x, -0.7f, pathPositionBuffer[i].position.y);
                    convertedPathPositions.Add(startPos);
                }
                if (math.distance(playerTransform.Position, targetPosition) < 1f)
                {
                    pathPositionBuffer.RemoveAt(pathFollow.pathIndex);
                }

                float3 convertedPlayer = new float3
                {
                    x = playerTransform.Position.x,
                    y = -0.7f,
                    z = playerTransform.Position.z,
                };

                float3 convertedTarget = new float3
                {
                    x = targetTransform.Position.x,
                    y = -0.7f,
                    z = targetTransform.Position.z,
                };

                convertedPathPositions[0] = convertedPlayer;
                convertedPathPositions[convertedPathPositions.Length - 1] = convertedTarget;
            }
            OnPathFind?.Invoke(convertedPathPositions.ToArray(Allocator.Temp).ToArray());
        }).Run();
    }

    public void SetTarget(float3 transform, float3 target, Entity entity, DynamicBuffer<PathPosition> pathPositions, EntityCommandBuffer.ParallelWriter ecb, int sortKey)
    {
        pathPositions.Clear();
        float3 pos = new float3 { x = transform.x, y = transform.z, z = 0 };
        Grid<GridNode> grid = GridSystem.instance.grid;

        if (grid == null) { return; }

        float cellSize = grid.GetCellSize();

        grid.GetXY(pos + new float3(1, 0, 1) * cellSize * .5f, out int startX, out int startY);

        float3 convertedTargetPos = new float3 { x = target.x, y = target.z, z = 0 };

        grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * cellSize * .5f, out int endX, out int endY);

        ValidateGridPosition(ref startX, ref startY, grid);
        ValidateGridPosition(ref endX, ref endY, grid);
        if (EntityManager.HasComponent<PathfindingParams>(entity))
        {
            ecb.SetComponent(sortKey, entity, new PathfindingParams
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(endX, endY)
            });
        }
        else
        {
            ecb.AddComponent(sortKey, entity, new PathfindingParams
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(endX, endY)
            });
        }
    }

    private void ValidateGridPosition(ref int x, ref int y, Grid<GridNode> grid)
    {
        x = math.clamp(x, 0, grid.GetWidth() - 1);
        y = math.clamp(y, 0, grid.GetHeight() - 1);
    }
}
