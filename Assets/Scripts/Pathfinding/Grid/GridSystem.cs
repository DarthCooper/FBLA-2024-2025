using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

partial class GridSystem : SystemBase
{
    public static GridSystem instance;
    public Grid<GridNode> grid;
    public GridNode[,] gridArray;

    [BurstCompile]
    protected override void OnCreate()
    {
        instance = this;
        RequireForUpdate<GridData>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if(grid == null)
        {
            CreateGrid();
            CheckStaticObstacles();
        }
        CheckTakenCells();

        CheckMovingObstacles();
        /*
        # if UNITY_EDITOR
            if(FPSCounter.m_lastFramerate > 60)
            {
                displayGrid();
            }
        #endif
        */
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
        
    }
    
    public void CreateGrid()
    {
        foreach ((GridData data, Entity enitity) in SystemAPI.Query<GridData>().WithEntityAccess())
        {
            grid = new Grid<GridNode>((int)data.size.x, (int)data.size.z, data.cellSize, data.origin, (Grid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));
            gridArray = grid.GetGridArray();
        }
    }

    public void CheckTakenCells()
    {
        float cellSize = grid.GetCellSize();
        NativeList<TakenCells> cellsToRemove = new NativeList<TakenCells>(Allocator.Temp);
        NativeList<int> indexes = new NativeList<int>(Allocator.Temp);
        foreach((DynamicBuffer<TakenCells> takenCells, Entity entity) in SystemAPI.Query<DynamicBuffer<TakenCells>>().WithEntityAccess())
        {
            foreach(TakenCells takenCell in takenCells)
            {
                if(takenCell.style == GridNodeStyle.Permanant) { continue; }
                if(takenCell.safe == true) { return; }
                int x = takenCell.position.x;
                int y = takenCell.position.y;
                ValidateGridPosition(ref x, ref y, grid);
                GridNode node = grid.GetGridObject(x, y);
                node.SetIsWalkable(true);
                cellsToRemove.Add(new TakenCells
                {
                    position = takenCell.position,
                });
            }
            for (int i = 0; i < takenCells.Length; i++)
            {
                foreach(TakenCells index in cellsToRemove)
                {
                    if (takenCells[i].position.x == index.position.x && takenCells[i].position.y == index.position.y)
                    {
                        indexes.Add(i);
                    }
                }
            }
            foreach (int index in indexes)
            {
                if(index >= takenCells.Length) { continue; }
                takenCells.RemoveAt(index);
            }
        }
    }

    public bool CheckTakenCells(int2 pos)
    {
        foreach ((DynamicBuffer<TakenCells> takenCells, Entity entity) in SystemAPI.Query<DynamicBuffer<TakenCells>>().WithEntityAccess())
        {
            foreach (TakenCells takenCell in takenCells)
            {
                if(takenCell.position.Equals(pos))
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void CheckMovingObstacles()
    {
        foreach ((LocalToWorld transform, DynamicBuffer<TakenCells> takenCells, Entity entity) in SystemAPI.Query<LocalToWorld, DynamicBuffer<TakenCells>>().WithAll<MovingObstacleTag>().WithEntityAccess())
        {
            float cellSize = grid.GetCellSize();
            float3 origin = new float3
            {
                x = transform.Position.x - (transform.Value.Scale().x / 2),
                y = 0,
                z = transform.Position.z - (transform.Value.Scale().z / 2)
            };
            int width = (int)System.Math.Ceiling(transform.Value.Scale().x / cellSize);
            int height = (int)System.Math.Ceiling(transform.Value.Scale().z / cellSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int worldX = Mathf.RoundToInt(origin.x + (x * cellSize));
                    int worldY = Mathf.RoundToInt(origin.z + (y * cellSize));

                    ValidateGridPosition(ref worldX, ref worldY, grid);

                    CheckGridPosition(ref worldX, ref worldY, cellSize, entity);

                    TakenCells newCell = new TakenCells
                    {
                        position = new int2
                        {
                            x = worldX,
                            y = worldY,
                        },
                        safe = false,
                    };


                    bool contained = false;
                    for (int i = 0; i < takenCells.Length; i ++)
                    {
                        TakenCells oldCell = takenCells[i];
                        if (oldCell.Equals(newCell))
                        {
                            oldCell.safe = true;
                            contained = true;
                        }
                    }
                    if (contained) { continue; }

                    takenCells.Add(new TakenCells
                    {
                        position = new int2
                        {
                            x = worldX,
                            y = worldY,
                        },
                        style = GridNodeStyle.Dynamic,
                    });
                }
            }
        }
    }

    public void CheckStaticObstacles()
    {
        foreach ((LocalToWorld transform, DynamicBuffer<TakenCells> takenCells, Aura aura, Entity entity) in SystemAPI.Query<LocalToWorld, DynamicBuffer<TakenCells>, Aura>().WithAll<StaticObstacleTag>().WithEntityAccess())
        {
            float cellSize = grid.GetCellSize();
            float3 origin = new float3
            {
                x = transform.Position.x - (transform.Value.Scale().x / 2) - aura.Value.x,
                y = 0,
                z = transform.Position.z - (transform.Value.Scale().z / 2) - aura.Value.z
            };
            int width = (int)System.Math.Ceiling(transform.Value.Scale().x / cellSize) + (2 * aura.Value.x);
            int height = (int)System.Math.Ceiling(transform.Value.Scale().z / cellSize) + (2 * aura.Value.z);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int worldX = Mathf.RoundToInt(origin.x + (x * cellSize));
                    int worldY = Mathf.RoundToInt(origin.z + (y * cellSize));

                    ValidateGridPosition(ref worldX, ref worldY, grid);

                    CheckGridPosition(ref worldX, ref worldY, cellSize, entity);

                    TakenCells newCell = new TakenCells
                    {
                        position = new int2
                        {
                            x = worldX,
                            y = worldY,
                        },
                        safe = false,
                    };


                    bool contained = false;
                    for (int i = 0; i < takenCells.Length; i++)
                    {
                        TakenCells oldCell = takenCells[i];
                        if (oldCell.Equals(newCell))
                        {
                            oldCell.safe = true;
                            contained = true;
                        }
                    }
                    if (contained) { continue; }

                    takenCells.Add(new TakenCells
                    {
                        position = new int2
                        {
                            x = worldX,
                            y = worldY,
                        },
                        style = GridNodeStyle.Permanant
                    });
                }
            }
        }
    }

    public void SetGrid()
    {
        if(grid == null) { CreateGrid(); return; }
        foreach ((GridData data, Entity entity) in SystemAPI.Query<GridData>().WithEntityAccess())
        {
            int width = (int)data.size.x;
            int height = (int)data.size.z;
            float cellSize = data.cellSize;

            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    CheckGridPosition(ref x, ref y, cellSize, entity);
                }
            }
        }
    }

    public void CheckGridPosition(ref int x, ref int y, float cellSize, Entity entity)
    {
        ValidateGridPosition(ref x, ref y, grid);

        float3 start = grid.GetWorldPosition(x, y);
        float3 end = grid.GetWorldPosition((int)(x + cellSize), (int)(y + cellSize));

        Entity hit = BoxCast(start, end, cellSize, CollisionFilters.filterSolid);
        GridNode node = grid.GetGridObject(x, y);
        if (hit != Entity.Null)
        {
            if (SystemAPI.HasComponent<IsWalkable>(hit))
            {
                RefRO<IsWalkable> type = SystemAPI.GetComponentRO<IsWalkable>(hit);
                switch (type.ValueRO.Value)
                {
                    case WalkableTypes.nonWalkable:
                        node.SetIsWalkable(false);
                        break;
                    case WalkableTypes.walkable:
                        break;
                    case WalkableTypes.nonWalkableAllower:
                        node.SetIsWalkable(false);
                        node.SetAllowedEntity(hit);
                        break;
                }
            }
        }
        else
        {
            node.SetIsWalkable(true);
            node.SetAllowedEntity(Entity.Null);
        }
    }

    public void displayGrid()
    {
        int width = grid.GetWidth();
        int height = grid.GetHeight();
        TextMesh[,] debugTextArray = new TextMesh[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Color color = Color.white;
                if(!grid.GetGridObject(x, y).IsWalkable())
                {
                    color = Color.red;
                    if(grid.GetGridObject(x, y).GetAllowedEntity() != Entity.Null)
                    {
                        color = Color.yellow;
                    }
                }

                //debugTextArray[x, y] = UtilsClass.CreateWorldText(gridArray[x, y].ToString(), null, GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * 0.5f, 20, Color.white, TextAnchor.MiddleCenter);
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x, y + 1), color, 10000000f);
                Debug.DrawLine(grid.GetWorldPosition(x, y), grid.GetWorldPosition(x + 1, y), color, 10000000f);
            }
        }
        Debug.DrawLine(grid.GetWorldPosition(0, height), grid.GetWorldPosition(width, height), Color.white, 10000000f);
        Debug.DrawLine(grid.GetWorldPosition(width, 0), grid.GetWorldPosition(width, height), Color.white, 10000000f);
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

    private void ValidateGridPosition(ref int x, ref int y, Grid<GridNode> grid)
    {
        x = math.clamp(x, 0, grid.GetWidth() - 1);
        y = math.clamp(y, 0, grid.GetHeight() - 1);
    }
}