using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
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
        foreach ((RefRW<IsFollowing> following, PathFollowTargetDistance targetDistance, PathFollowTarget target, PathFollowerPreviousTarget lastTarget, LocalTransform transform, Entity entity) in SystemAPI.Query<RefRW<IsFollowing>, PathFollowTargetDistance, PathFollowTarget, PathFollowerPreviousTarget, LocalTransform>().WithEntityAccess())
        {
            if(target.Value.Equals(Entity.Null)) { CheckScouting(entity, transform, target); continue; }
            if(!following.ValueRO.Value) { following.ValueRW.Value = true; }
            if(!EntityManager.HasComponent<PathStartedTag>(entity))
            {
                ecb.AddComponent<PathStartedTag>(entity);
            }
            SetTarget(transform, target, entity, EntityManager.GetBuffer<PathPosition>(entity));
            LocalTransform targetTransform = SystemAPI.GetComponent<LocalTransform>(target.Value);
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            CheckRetreating(dist, entity, transform, target);
            if (dist < targetDistance.Value || EntityManager.GetComponentData<PathFollow>(entity).pathIndex == -1)
            {
                ecb.AddComponent<AtTarget>(entity);
                SetTarget(transform, target, entity, EntityManager.GetBuffer<PathPosition>(entity));
                following.ValueRW.Value = false;
                if(EntityManager.HasComponent<Retreating>(entity))
                {
                    ecb.DestroyEntity(target.Value);
                    ecb.RemoveComponent<Retreating>(entity);
                    ecb.AddComponent<Hunting>(entity);
                    ChangeTarget(entity, Entity.Null, lastTarget.Value, EntityManager.GetComponentData<PathFollowerPreviousTargetDistance>(entity).Value);
                }
                if(EntityManager.HasComponent<Scouting>(entity))
                {
                    ecb.DestroyEntity(target.Value);
                    ecb.RemoveComponent<Scouting>(entity);
                    ecb.SetComponent(entity, new PathFollowTarget
                    {
                        Value = Entity.Null
                    });
                }
            }else if(EntityManager.HasComponent<AtTarget>(entity))
            {
                ecb.RemoveComponent<AtTarget>(entity);
            }
        }
        ecb.Playback(EntityManager);
    }

    public void CheckRetreating(float dist, Entity entity, LocalTransform transform, PathFollowTarget target, float subtractor = 0)
    {
        if(!EntityManager.HasComponent<PathFollowRetreatDistances>(entity)) { return; }
        if(EntityManager.HasComponent<Scouting>(entity)) { return; }
        PathFollowRetreatDistances retreatDistances = EntityManager.GetComponentData<PathFollowRetreatDistances>(entity);
        if (dist <= retreatDistances.Trigger && !EntityManager.HasComponent<Retreating>(entity))
        {
            Grid<GridNode> grid = GridSystem.instance.grid;
            if (grid == null) { return; }

            float3 targetPos = EntityManager.GetComponentData<LocalTransform>(target.Value).Position;

            int retreatDistance = (int)UnityEngine.Random.Range(retreatDistances.Min - subtractor, retreatDistances.Max - subtractor);
            float3 retreatDir = transform.Position - targetPos;
            Entity retreatEntity = EntityManager.CreateEntity();

            float3 targetGoal = transform.Position + (float3)Vector3.Normalize(retreatDir) * retreatDistance;

            float3 convertedTargetPos = new float3 { x = targetGoal.x, y = targetGoal.z, z = 0 };

            grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * grid.GetCellSize() * .5f, out int endX, out int endY);
            ValidateGridPosition(ref endX, ref endY, grid);

            ecb.AddComponent(retreatEntity, new LocalTransform
            {
                Position = grid.GetWorldPosition(endX, endY),
                Rotation = Quaternion.identity,
                Scale = 1
            });

            ChangeTarget(entity, target.Value, retreatEntity, 1f);
            ecb.AddComponent<Retreating>(entity);
            if(EntityManager.HasComponent<Hunting>(entity))
            {
                ecb.RemoveComponent<Hunting>(entity);
            }
            if(EntityManager.HasComponent<Scouting>(entity))
            {
                ecb.RemoveComponent<Scouting>(entity);
            }
        }
    }

    public void CheckScouting(Entity entity, LocalTransform transform, PathFollowTarget target)
    {
        if (!EntityManager.HasComponent<PathFollowerScoutingDistances>(entity)) { return; }
        if(EntityManager.HasComponent<Retreating>(entity)) { return; }
        if (EntityManager.HasComponent<Hunting>(entity)) { return; }
        PathFollowerScoutingDistances scoutDistances = EntityManager.GetComponentData<PathFollowerScoutingDistances>(entity);
        Grid<GridNode> grid = GridSystem.instance.grid;
        if (grid == null) { return; }
        int scoutDistanceX = (int)UnityEngine.Random.Range(scoutDistances.Min, scoutDistances.Max);
        int scoutDistanceY = (int)UnityEngine.Random.Range(scoutDistances.Min, scoutDistances.Max);
        Entity scoutEntity = EntityManager.CreateEntity();

        float3 targetGoal = transform.Position + new float3 { x = scoutDistanceX, y = 0, z = scoutDistanceY };

        float3 convertedTargetPos = new float3 { x = targetGoal.x, y = targetGoal.z, z = 0 };

        grid.GetXY(convertedTargetPos + new float3(1, 0, 1) * grid.GetCellSize() * .5f, out int endX, out int endY);
        ValidateGridPosition(ref endX, ref endY, grid);
        if(!GridSystem.instance.CheckTakenCells(new int2 { x = endX, y = endY}))
        {
            CheckScouting(entity, transform, target);
        }

        ecb.AddComponent(scoutEntity, new LocalTransform
        {
            Position = grid.GetWorldPosition(endX, endY),
            Rotation = Quaternion.identity,
            Scale = 1
        });

        ChangeTarget(entity, target.Value, scoutEntity, 1f);
        ecb.AddComponent<Scouting>(entity);
        if (EntityManager.HasComponent<Hunting>(entity))
        {
            ecb.RemoveComponent<Hunting>(entity);
        }
        if(EntityManager.HasComponent<Retreating>(entity))
        {
            ecb.RemoveComponent<Retreating>(entity);
        }
    }

    public void ChangeTarget(Entity entity, Entity target, Entity newTarget, float newDistance)
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
    }

    public void SetTarget(LocalTransform transform, PathFollowTarget target, Entity entity, DynamicBuffer<PathPosition> pathPositions)
    {
        if(target.Value == Entity.Null) { return; }
        pathPositions.Clear();
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
        if(EntityManager.HasComponent<PathfindingParams>(entity))
        {
            ecb.SetComponent(entity, new PathfindingParams
            {
                startPosition = new int2(startX, startY),
                endPosition = new int2(endX, endY)
            });
        }else
        {
            ecb.AddComponent(entity, new PathfindingParams
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

    [BurstCompile]
    protected override void OnDestroy()
    {
    }

    public Entity Raycast(float3 RayFrom, float3 RayTo)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        RaycastInput input = new RaycastInput()
        {
            Start = RayFrom,
            End = RayTo,
            Filter = new CollisionFilter()
            {
                BelongsTo = ~0u,
                CollidesWith = ~0u, // all 1s, so all layers, collide with everything
                GroupIndex = 0
            }
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
