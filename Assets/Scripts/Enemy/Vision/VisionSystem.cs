using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial struct VisionSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PathFollowTarget>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Temp);

        foreach((LocalToWorld tranform, PhysicsVelocity velocity ,Entity entity) in SystemAPI.Query<LocalToWorld, PhysicsVelocity>().WithEntityAccess())
        {
            if(velocity.Linear.Equals(Vector3.zero)) { continue; }
            float3 playerPos = GetPlayerPos(ref state);
            if (state.EntityManager.HasComponent<Scouting>(entity))
            {
                if (IsPointInTriangle(playerPos, tranform.Position, velocity.Linear, 6f, 12f))
                {
                    ecb.RemoveComponent<Scouting>(entity);
                    ecb.AddComponent<Hunting>(entity);
                    ecb.SetComponent(entity, new PathFollowTarget
                    {
                        Value = GetPlayer()
                    });
                }
            }
            if (state.EntityManager.HasComponent<Hunting>(entity))
            {
                if(!IsPointInCircle(playerPos, tranform.Position, 17f))
                {
                    ecb.RemoveComponent<Hunting>(entity);
                    ecb.AddComponent<Scouting>(entity);
                    ecb.SetComponent(entity, new PathFollowTarget
                    {
                        Value = Entity.Null
                    });
                }
            }
        }
        ecb.Playback(state.EntityManager);
    }

    bool IsPointInTriangle(Vector3 point, Vector3 apex, Vector3 direction, float baseWidth, float height)
    {
        // Calculate the base points
        Vector3 normalizedDir = direction.normalized;
        Vector3 right = Vector3.Cross(normalizedDir, Vector3.up).normalized * baseWidth;
        Vector3 baseMidpoint = apex + normalizedDir * height;
        Vector3 baseLeft = baseMidpoint - right;
        Vector3 baseRight = baseMidpoint + right;

        // Check using area method
        float totalArea = TriangleArea(apex, baseLeft, baseRight);
        float area1 = TriangleArea(point, baseLeft, baseRight);
        float area2 = TriangleArea(apex, point, baseRight);
        float area3 = TriangleArea(apex, baseLeft, point);

        bool foundPlayer = Mathf.Abs(totalArea - (area1 + area2 + area3)) < 0.001f; // Use a small tolerance for floating-point errors
        #if UNITY_EDITOR
        Debug.DrawLine(baseLeft, baseRight, foundPlayer ? Color.red : Color.magenta);
        Debug.DrawLine(apex, baseRight, foundPlayer ? Color.red : Color.magenta);
        Debug.DrawLine(apex, baseLeft, foundPlayer ? Color.red : Color.magenta);
        return foundPlayer;
        #endif
    }

    float TriangleArea(Vector3 a, Vector3 b, Vector3 c)
    {
        return Mathf.Abs((a.x * (b.z - c.z) + b.x * (c.z - a.z) + c.x * (a.z - b.z)) / 2f);
    }

    bool IsPointInCircle(Vector3 point, Vector3 center, float radius)
    {
        // Calculate the distance squared from the point to the center
        float distanceSquared = (point - center).sqrMagnitude;

        // Check if the distance squared is less than or equal to the radius squared
        DrawCircle(center, radius, 100);
        return distanceSquared <= radius * radius;
    }

    void DrawCircle(Vector3 center, float radius, int segments)
    {
        #if UNITY_EDITOR
        float angleStep = 360f / segments;
        Vector3 prevPoint = center + new Vector3(radius, 0, 0); // Start point

        for (int i = 1; i <= segments; i++)
        {
            float angle = angleStep * i;
            Vector3 newPoint = center + new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, 0, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);

            Debug.DrawLine(prevPoint, newPoint, Color.red);
            prevPoint = newPoint;
        }
        #endif
    }

    public float3 GetPlayerPos(ref SystemState state)
    {
        Entity playerEntity = SystemAPI.GetSingletonEntity<PlayerTag>();
        LocalToWorld playerTransform = SystemAPI.GetComponent<LocalToWorld>(playerEntity);
        return playerTransform.Position;
    }

    public Entity GetPlayer()
    {
        return SystemAPI.GetSingletonEntity<PlayerTag>();
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }

    public Entity Raycast(float3 RayFrom, float3 RayTo, ref SystemState state)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = state.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        RaycastInput input = new RaycastInput()
        {
            Start = RayFrom,
            End = RayTo,
            Filter = CollisionFilters.filterSolid
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        bool haveHit = collisionWorld.CastRay(input, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }
}
