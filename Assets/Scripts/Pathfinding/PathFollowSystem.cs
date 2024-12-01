using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowSystem : SystemBase
{
    NativeList<int2> enemyPositions = new NativeList<int2>(Allocator.Persistent);

    protected override void OnUpdate()
    {
        Grid<GridNode> grid = GridSystem.instance.grid;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities.WithoutBurst().ForEach((Entity entity, ref PathFollowSpeed speed, ref DynamicBuffer<PathPosition> pathPositionBuffer, ref LocalTransform transform, ref PhysicsVelocity velocity, ref PhysicsMass mass, ref PathFollow pathFollow) =>
        {
            if(!EntityManager.GetComponentData<IsFollowing>(entity).Value) { return; }
            if(EntityManager.HasComponent<Stunned>(entity)) { return; }
            if(pathFollow.pathIndex != pathPositionBuffer.Length - 2)
            {
                pathFollow.pathIndex = pathPositionBuffer.Length - 2;
            }
            if (pathFollow.pathIndex <= -1)
            {
                pathFollow.pathIndex = -1;
                velocity.Linear = 0;
                return;
            }
            if (pathFollow.pathIndex >= 0 && pathFollow.pathIndex < pathPositionBuffer.Length && pathPositionBuffer.Length > 0)
            {
                int2 pathPosition = pathPositionBuffer[pathFollow.pathIndex].position;

                //grid.GetGridObject(pathPosition.x, pathPosition.y).SetIsWalkable(false);
               // enemyPositions.Add(pathPosition);

                float3 targetPosition = new float3(pathPosition.x, 0, pathPosition.y);
                float3 moveDir = math.normalize(targetPosition - transform.Position);
                float moveSpeed = speed.Value;

                ecb.SetComponent(entity, new Direction { Value =  moveDir });
                velocity.Linear = moveDir * moveSpeed;

                #if UNITY_EDITOR
                for (int i = pathPositionBuffer.Length - 1; i > 0; i--)
                {
                    Vector3 startPos = new Vector3(pathPositionBuffer[i].position.x, 0, pathPositionBuffer[i].position.y);
                    Vector3 endPos = new Vector3(pathPositionBuffer[i - 1].position.x, 0, pathPositionBuffer[i - 1].position.y);

                    Debug.DrawLine(startPos, endPos, Color.green, 0.01f);
                }
                #endif
                if (math.distance(transform.Position, targetPosition) < 1f)
                {
                    //transform.Position = new float3(targetPosition.x, transform.Position.y, targetPosition.z);
                    pathPositionBuffer.RemoveAt(pathFollow.pathIndex);
                }
            }
        }).Run();

        ecb.Playback(EntityManager);
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
