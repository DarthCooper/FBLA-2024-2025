using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

partial class GridSystem : SystemBase
{
    public Grid grid;

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<GridData>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if(grid == null)
        {
            CreateGrid();
        }
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
    
    public void CreateGrid()
    {
        foreach ((GridData data, Entity enitity) in SystemAPI.Query<GridData>().WithEntityAccess())
        {
            grid = new Grid((int)data.size.x, (int)data.size.z, data.cellSize, data.origin);
        }

        SetGrid();
    }

    public void SetGrid()
    {
        if(grid == null) { CreateGrid(); return; }
        foreach ((GridData data, Entity enitity) in SystemAPI.Query<GridData>().WithEntityAccess())
        {
            int width = (int)data.size.x;
            int height = (int)data.size.y;
            float cellSize = data.cellSize;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float3 start = new float3
                    {
                        x = x,
                        y = 0,
                        z = y
                    };

                    float3 end = new float3
                    {
                        x = x + cellSize,
                        y = 0,
                        z = y + cellSize
                    };

                    Entity hit = BoxCast(start, end, cellSize, CollisionFilters.filterSolid);
                    if (hit != Entity.Null)
                    {
                        if(SystemAPI.HasComponent<IsWalkable>(hit))
                        {
                            RefRO<IsWalkable> type = SystemAPI.GetComponentRO<IsWalkable>(hit);
                            switch (type.ValueRO.Value)
                            {
                                case WalkableTypes.nonWalkable:
                                    Debug.Log("Disabling " + x + "" + y);
                                    grid.SetValue(x, y, 1);
                                    break;
                                case WalkableTypes.walkable:
                                    break;
                            }
                        }
                    }
                }
            }
        }
    }

    public unsafe Entity BoxCast(float3 RayFrom, float3 RayTo, float size, CollisionFilter filter)
    {
        // Set up Entity Query to get PhysicsWorldSingleton
        // If doing this in SystemBase or ISystem, call GetSingleton<PhysicsWorldSingleton>()/SystemAPI.GetSingleton<PhysicsWorldSingleton>() directly.
        EntityQueryBuilder builder = new EntityQueryBuilder(Allocator.Temp).WithAll<PhysicsWorldSingleton>();

        EntityQuery singletonQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(builder);
        var collisionWorld = singletonQuery.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
        singletonQuery.Dispose();

        BoxGeometry boxGeometry = new BoxGeometry() { Center = float3.zero, Orientation = Quaternion.identity, Size = size };
        BlobAssetReference<Unity.Physics.Collider> boxCollider = Unity.Physics.BoxCollider.Create(boxGeometry, filter);

        ColliderCastInput input = new ColliderCastInput()
        {
            Collider = (Unity.Physics.Collider*)boxCollider.GetUnsafePtr(),
            Orientation = quaternion.identity,
            Start = RayFrom,
            End = RayTo
        };

        ColliderCastHit hit = new ColliderCastHit();
        bool haveHit = collisionWorld.CastCollider(input, out hit);
        if (haveHit)
        {
            return hit.Entity;
        }

        boxCollider.Dispose();

        return Entity.Null;
    }
}