using System.Net;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref DynamicBuffer<PathPosition> pathPositionBuffer, ref PathfindingParams pathfindingParams,ref LocalTransform transform, ref PathFollow pathFollow) =>
        {
            if(pathFollow.pathIndex == -1)
            {
                //reached destination
            }
            if(pathFollow.pathIndex >= 0 && pathPositionBuffer.Length > 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

                float3 targetPosition = new float3(pathPosition.x, 0, pathPosition.y);
                float3 moveDir = math.normalizesafe(targetPosition - transform.Position);
                float moveSpeed = 5f;

                transform.Position += moveDir * moveSpeed * SystemAPI.Time.DeltaTime;

                if (math.distance(transform.Position, targetPosition) < .1f)
                {
                    pathFollow.pathIndex -= 1;
                    pathfindingParams.startPosition = pathPosition;
                }
            }
        }).Schedule();
    }

    private static void ValidateGridPosition(ref int x, ref int y, int width, int height)
    {
        x = math.clamp(x, 0, width - 1);
        y = math.clamp(y, 0, height - 1);
    }

    private static void GetXY(float3 worldPosition, float3 originPosition, float cellSize, out int x, out int y)
    {
        x = (int)math.floor((worldPosition - originPosition).x / cellSize);
        y = (int)math.floor((worldPosition - originPosition).y / cellSize);
    }
}
