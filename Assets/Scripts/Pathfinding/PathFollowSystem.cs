using System.Net;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((DynamicBuffer<PathPosition> pathPositionBuffer, ref LocalTransform transform, ref PathFollow pathFollow) =>
        {
            if(pathFollow.pathIndex >= 0 && pathPositionBuffer.Length > 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

                float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                float3 moveDir = math.normalizesafe(targetPosition - transform.Position);
                float moveSpeed = 5f;

                transform.Position += moveDir * moveSpeed * SystemAPI.Time.DeltaTime;

                if(math.distance(transform.Position, targetPosition) < .1f)
                {
                    pathFollow.pathIndex--;
                }
            }
        }).Schedule();
    }
}
