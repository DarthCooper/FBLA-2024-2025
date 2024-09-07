using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Windows;

partial struct PlayerMoveSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<PlayerMoveInput>();
        state.RequireForUpdate<PlayerMoveSpeed>();
        state.RequireForUpdate<PhysicsVelocity>();
    }

    public void OnUpdate(ref SystemState state)
    {
        SetPlayerChecks(ref state);

        ClampPlayerInput(ref state);

        SetPlayerMovement(ref state);
    }

    public void ClampPlayerInput(ref SystemState state)
    {
        foreach(var (checks, moveInput, jumpInput, entity) in SystemAPI.Query<PlayerChecks, RefRW<PlayerMoveInput>, RefRW<PlayerJumpInput>>().WithEntityAccess())
        {
            if(!checks.groundCheck && jumpInput.ValueRO.Value)
            {
                jumpInput.ValueRW.Value = false;
            }

            float horizontalInput = (float)math.clamp(moveInput.ValueRO.Value.x, -Convert.ToDouble(!checks.leftWallCheck), Convert.ToDouble(!checks.rightWallCheck));
            float verticalInput = (float)math.clamp(moveInput.ValueRO.Value.y, -Convert.ToDouble(!checks.forwardCheck), Convert.ToDouble(!checks.backCheck));

            moveInput.ValueRW.Value = new float2
            {
                x = horizontalInput,
                y = verticalInput,
            };
        }
    }

    public void SetPlayerMovement(ref SystemState state)
    {
        foreach (var (velocity, moveInput, jumpInput, moveSpeed, jumpForce, entity) in SystemAPI.Query<RefRW<PhysicsVelocity>, PlayerMoveInput, PlayerJumpInput, PlayerMoveSpeed, PlayerJumpForce>().WithEntityAccess())
        {
            float3 curVel = velocity.ValueRO.Linear;
            float2 moveVector = moveInput.Value * moveSpeed.Value;
            float jumpSpeed = jumpInput.Value ? jumpForce.Value : curVel.y;
            velocity.ValueRW.Linear = new float3(moveVector.x, jumpSpeed, moveVector.y);
        }
    }

    public void SetPlayerChecks(ref SystemState state)
    {
        foreach(var (transform, checks, offsets, entity) in SystemAPI.Query<LocalTransform, RefRW<PlayerChecks>, PlayerChecksOffset>().WithEntityAccess())
        {
            checks.ValueRW.groundCheck = (PlayerRayChecks.GroundCheck(new LocalTransform
            {
                Position = new float3 {
                    x = transform.Position.x,
                    y = transform.Position.y + offsets.Value.x,
                    z = transform.Position.z
                }
            },
            0.25f, CollisionFilters.filterCharacter) != Entity.Null);

            checks.ValueRW.leftWallCheck = (PlayerRayChecks.LeftWallCheck(new LocalTransform
            {
                Position = new float3
                {
                    x = transform.Position.x + offsets.Value.y,
                    y = transform.Position.y,
                    z = transform.Position.z
                }
            },
            0.25f, CollisionFilters.filterCharacter) != Entity.Null);

            checks.ValueRW.rightWallCheck = (PlayerRayChecks.RightWallCheck(new LocalTransform
            {
                Position = new float3
                {
                    x = transform.Position.x + offsets.Value.z,
                    y = transform.Position.y,
                    z = transform.Position.z
                }
            },
            0.25f, CollisionFilters.filterCharacter) != Entity.Null);

            checks.ValueRW.ceilingCheck = (PlayerRayChecks.CeilingCheck(new LocalTransform
            {
                Position = new float3
                {
                    x = transform.Position.x,
                    y = transform.Position.y + offsets.Value.w,
                    z = transform.Position.z
                }
            },
            0.25f, CollisionFilters.filterCharacter) != Entity.Null);

            checks.ValueRW.forwardCheck = (PlayerRayChecks.FrontCheck(new LocalTransform
            {
                Position = transform.Position
            },
            0.35f, CollisionFilters.filterCharacter) != Entity.Null);

            checks.ValueRW.backCheck = (PlayerRayChecks.BackCheck(new LocalTransform
            {
                Position = transform.Position
            },
            0.35f, CollisionFilters.filterCharacter) != Entity.Null);
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
}

public class PlayerRayChecks
{
    public static Entity GroundCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (-transform.Up() + transform.Position) * 1f,
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (-transform.Up() + transform.Position) * 1, Color.red);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity LeftWallCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (-transform.Right() + transform.Position),
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (-transform.Right() + transform.Position) * 1, Color.green);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity RightWallCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (transform.Right() + transform.Position),
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (transform.Right() + transform.Position) * 1, Color.blue);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity CeilingCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (transform.Up() + transform.Position),
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (transform.Up() + transform.Position) * 1, Color.yellow);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity FrontCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (-transform.Forward() + transform.Position),
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (-transform.Forward() + transform.Position) * 1, Color.white);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }

    public static Entity BackCheck(LocalTransform transform, float length, CollisionFilter filter)
    {
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        var raycastInput = new RaycastInput
        {
            Start = transform.Position,
            End = (transform.Forward() + transform.Position),
            Filter = filter
        };

        Unity.Physics.RaycastHit hit = new Unity.Physics.RaycastHit();
        Debug.DrawLine(transform.Position, (transform.Forward() + transform.Position) * 1, Color.magenta);
        bool haveHit = collisionWorld.CastRay(raycastInput, out hit);
        if (haveHit && math.distance(transform.Position, hit.Position) < length)
        {
            return hit.Entity;
        }
        return Entity.Null;
    }
}
