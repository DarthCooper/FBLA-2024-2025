using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial class PathFollowSystem : SystemBase
{
    public Action<float3, Entity> OnMove;
    protected override void OnUpdate()
    {
        Grid<GridNode> grid = GridSystem.instance.grid;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities.WithoutBurst().ForEach((Entity entity, ref PathFollowSpeed speed, ref DynamicBuffer<PathPosition> pathPositionBuffer, ref LocalTransform transform, ref PhysicsVelocity velocity, ref PhysicsMass mass, ref PathFollow pathFollow) =>
        {
            if(!EntityManager.GetComponentData<IsFollowing>(entity).Value) { return; }
            if(EntityManager.HasComponent<Stunned>(entity)) { return; }
            if(EntityManager.HasComponent<Casting>(entity)) { return; }
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

                LocalToWorld worldTransform = EntityManager.GetComponentData<LocalToWorld>(entity);
                OnMove?.Invoke(worldTransform.Position, entity);

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
}
