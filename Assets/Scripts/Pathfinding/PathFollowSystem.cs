using System.Net;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowSystem : SystemBase
{
    NativeList<int2> enemyPositions = new NativeList<int2>(Allocator.Persistent);

    protected override void OnUpdate()
    {
        Grid<GridNode> grid = GridSystem.instance.grid;

        Entities.WithoutBurst().ForEach((Entity entity, ref PathFollowSpeed speed, ref DynamicBuffer<PathPosition> pathPositionBuffer, ref PathfindingParams pathfindingParams,ref LocalTransform transform, ref PhysicsVelocity velocity, ref PhysicsMass mass, ref PathFollow pathFollow) =>
        {
            if(!EntityManager.GetComponentData<IsFollowing>(entity).Value) { return; }
            if(pathFollow.pathIndex != pathPositionBuffer.Length - 2)
            {
                pathFollow.pathIndex = pathPositionBuffer.Length - 2;
            }
            if (pathFollow.pathIndex <= -1)
            {
                pathFollow.pathIndex = -1;
                return;
            }
            if (pathFollow.pathIndex >= 0 && pathFollow.pathIndex < pathPositionBuffer.Length && pathPositionBuffer.Length > 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

                //grid.GetGridObject(pathPosition.x, pathPosition.y).SetIsWalkable(false);
               // enemyPositions.Add(pathPosition);

                float3 targetPosition = new float3(pathPosition.x, 0, pathPosition.y);
                float3 moveDir = math.normalizesafe(targetPosition - transform.Position);
                float moveSpeed = speed.Value;

                velocity.Linear = moveDir * moveSpeed;

                #if UNITY_EDITOR
                for (int i = pathFollow.pathIndex + 1; i < pathPositionBuffer.Length; i++)
                {
                    Vector3 startPos = new Vector3(pathPositionBuffer[i - 1].position.x, 0, pathPositionBuffer[i - 1].position.y);
                    Vector3 endPos = new Vector3(pathPositionBuffer[i].position.x, 0, pathPositionBuffer[i].position.y);

                    Debug.DrawLine(startPos, endPos, Color.green, 60 / FPSCounter.m_lastFramerate);
                }
                #endif
                if (math.distance(transform.Position, targetPosition) < .1f)
                {
                    pathPositionBuffer.RemoveAt(pathFollow.pathIndex);
                    pathfindingParams.startPosition = pathPosition;
                }
            }
        }).Run();
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
